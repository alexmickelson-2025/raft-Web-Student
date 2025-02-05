using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace library;

public enum States
{
    Follower,
    Candidate,
    Leader
};

public class Server : IServer
{
    public States State { get; set; }
    public int ElectionTimeout { get; set; } //Specifies the Election Timeout in milisecondss
    public IServer? RecognizedLeader { get; set; }
    public Dictionary<int, List<IServer>> AppendEntriesResponseLog = new();
    //public Dictionary<int, <bool> AppendEntriesResponseLog = new();
    public int CurrentTerm { get; set; }
    public int Id { get; set; }

    public int ElectionTimeoutAdjustmentFactor { get; set; } //default value of 1
    public int NetworkDelay { get; set; } = 0;
    public List<RaftLogEntry> LogBook { get; set; }

    public List<IServer> OtherServersList { get; set; } = new();
    public Dictionary<IServer, int> NextIndex { get; set; } = new();
    public int HighestCommittedIndex { get; set; } = 0;
    public Dictionary<string, string> StateDictionary { get; set; } = new();
    public bool IsPaused { get; set; } = false;

    public Dictionary<int, Server> VotesCast = new(); //<termNumber, ServerWeVotedFor>

    public List<IServer> VotesReceived = new(); //If it has a Server in it, that means the server has voted for it with that current term.

    public Stopwatch timeSinceHearingFromLeader = new();
    private Stopwatch timeSinceLastSentHeartbeatAsLeader = new();
    public static readonly int IntervalAtWhichLeaderShouldSendHeartbeatsInMs = 50;

    public Server()
    {
        ElectionTimeoutAdjustmentFactor = 1;
        NetworkDelay = 0;
        this.State = States.Follower;
        this.LogBook = new();
        this.ResetElectionTimeout();
    }

    public Server(bool TrackTimeSinceHearingFromLeaderAndStartElectionBecauseOfIt, bool TrackTimeAtWhichLeaderShouldSendHeartbeats)
    {
        Console.WriteLine("called constructor to turn on a server");

        ElectionTimeoutAdjustmentFactor = 1;
        NetworkDelay = 0;
        this.State = States.Follower;
        this.LogBook = new();
        this.ResetElectionTimeout();
        if (TrackTimeSinceHearingFromLeaderAndStartElectionBecauseOfIt) {
            this.ResetElectionTimeout();
            this.timeSinceHearingFromLeader.Start();
            new Thread(() => StartBackgroundTaskToMonitorTimeSinceHearingFromLeaderAndStartNewElection()).Start();
        }
        if (TrackTimeAtWhichLeaderShouldSendHeartbeats)
        {
            //TODO: One day, I think I could turn this from just listening for a leader state to just one "listen for all states" that combines them or starts the three different threads
            new Thread(() => ListenForLeaderState()).Start();
        }
    }

    public void ResetElectionTimeout()
    {
        Console.WriteLine("Reset election timeout was called");
        int val = (Random.Shared.Next() % 150) + 150;
        ElectionTimeout = val * ElectionTimeoutAdjustmentFactor;
    }

    public void SendAppendEntriesLogTo(IServer follower)
    {
        Console.WriteLine("Called Send Append Entries Log To function");
        //make a new appendEntriesLog to pass along
        RaftLogEntry raftLogEntry = new RaftLogEntry()
        {
            Command = ("", ""),
            LeaderHighestCommittedIndex = this.HighestCommittedIndex,
            TermNumber = this.CurrentTerm,
            LogIndex = this.LogBook.Count//Todo this is the line I changed
           // LogIndex = this.LogBook.Count + 1 //Why do I need this to be plus 1? can I just do the count??? 
            //TODO: I'm not sure what to do about the log index. Is it really just the next available spot in the log book?
        };
        follower.ReceiveAppendEntriesLogFrom(this, [raftLogEntry]);
        //follower.ReceiveAppendEntriesLogFrom(this, 0, this.CurrentTerm, raftLogEntry); //I need to be able to automatically increment this
    }
    public void ReceiveAppendEntriesLogFrom(IServer leader, RaftLogEntry request) //delete this one in a minute
    {
        ReceiveAppendEntriesLogFrom(leader, [request]);
    }
    public void ReceiveAppendEntriesLogFrom(IServer server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null)
    {
        this.RecognizedLeader = server;
        if (logEntry == null) //This will come out in a minute when we're done removing all other parameters and just receiving a log entry and a server
        {
            if (requestCurrentTerm < this.CurrentTerm)
            {
                this.SendAppendEntriesResponseTo(server, requestNumber, false);
            }
            else
            {
                this.SendAppendEntriesResponseTo(server, requestNumber, true);
                this.State = States.Follower;
                this.timeSinceHearingFromLeader.Reset();
                this.timeSinceHearingFromLeader.Start();
            }
        }
        else
        {
            if (logEntry.TermNumber < this.CurrentTerm)
            {
                this.SendAppendEntriesResponseTo(server, requestNumber, false);
            }
            else
            {
                this.SendAppendEntriesResponseTo(server, requestNumber, true);
                this.State = States.Follower;
                this.timeSinceHearingFromLeader.Reset();
                this.timeSinceHearingFromLeader.Start();
            }
        }
    }

    private void SendAppendEntriesResponseTo(IServer server, int requestNumber, bool accepted)
    {
        var response = new AppendEntryResponse()
        {
            TermNumber = server.CurrentTerm,
            LogIndex = requestNumber,
            Accepted = accepted
        };
        server.ReceiveAppendEntriesLogResponseFrom(this, response);
        //server.ReceiveAppendEntriesLogResponseFrom(this, requestNumber, accepted);
    }

    public void ReceiveAppendEntriesLogResponseFrom(IServer server, AppendEntryResponse response)
    {
        //If our appendEntriesResponseWasREjected, send the previous one:
        if(!response.Accepted && response.TermNumber <= this.CurrentTerm)
        {
            if (response.LogIndex - 1 >= 0 && response.LogIndex - 1 < LogBook.Count())
            {
                server.ReceiveAppendEntriesLogFrom(this, [LogBook[response.LogIndex - 1]]);
            }
            return;
        }

        //ELSE, it was accepted, so we need to record that
        if (!AppendEntriesResponseLog.ContainsKey(response.LogIndex))
        {
            AppendEntriesResponseLog.Add(response.LogIndex, new List<IServer>() { server });
            //BUG TODO I crashed here one time adding the same request number twice. Perhaps I need locks on this?
        }
        else //at least 1 item is already in there, so we must append to the list
        {
            var list = AppendEntriesResponseLog[response.LogIndex];
            list.Add(server);
            AppendEntriesResponseLog[response.LogIndex] = list;
        }
        //Let's also add something to see if we have received a majority of responses and can commit the log
        if (AppendEntriesResponseLog[response.LogIndex].Count >= (OtherServersList.Count / 2 + 1)) //Because then we have a majority of responses.
        {
            CommitEntry(response.LogIndex); //this will call increment highest ccommitted index, and apply entry. I'm noticing here I'm passing it too high of a log index
        }
        //ReceiveAppendEntriesLogResponseFrom(server, response.LogIndex, response.Accepted);
    }

    //public void ReceiveAppendEntriesLogResponseFrom(IServer server, int requestNumber, bool accepted)
    //{

    //    //if (!AppendEntriesResponseLog.ContainsKey(requestNumber))
    //    //{
    //    //    AppendEntriesResponseLog.Add(requestNumber, accepted);
    //    //    //BUG TODO I crashed here one time adding the same request number twice. Perhaps I need locks on this?
    //    //}
    //    throw new Exception("We shouldn't have called this!");
    //}

    public void StartElection()
    {
        Console.WriteLine($"Starting Election, from term {this.CurrentTerm} to term {this.CurrentTerm +1}");
        this.VotesReceived = [this];
        this.State = States.Candidate;
        this.CurrentTerm++;
        //syntax from this stack overflow article https://stackoverflow.com/questions/4161120/how-should-i-create-a-background-thread
        //new Thread(() => NameOfYourMethod()).Start();

        //Rachel note: I think we need to call SendRequestForVoteRPCTo here!
        foreach (var follower in OtherServersList)
        {
            this.SendRequestForVoteRPCTo(follower);
        }
        new Thread(() => StartBackgroundTaskToMonitorElectionTimeoutAndStartNewElection()).Start();
        new Thread(() => StartBackgroundTaskToDetermineIfWeJustWonAnElection()).Start(); //Did we win?
    }

    public void SendRequestForVoteRPCTo(IServer server)
    {
        new Thread(() => server.ReceiveVoteRequestFrom(this, this.CurrentTerm)).Start();
        //server.ReceiveVoteRequestFrom(this, this.CurrentTerm);
    }

    //The reason we pass the requestedVote current term (even though it's a property on the server requesting) is the server requesting might update its term after we receive it, so we can't trust that property and must specify it
    public void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
        if (NetworkDelay > 0)
        {
            Thread.Sleep(NetworkDelay);
        }
        if (requestedVoteCurrentTerm > this.CurrentTerm)
        {
            if (!VotesCast.ContainsKey(requestedVoteCurrentTerm))
            {
                VotesCast.Add(requestedVoteCurrentTerm, serverRequesting);
                SendVoteResponseTo(serverRequesting, requestedVoteCurrentTerm, true);
            }
        }
    }

    private void SendVoteResponseTo(Server serverRequesting, int requestedVoteCurrentTerm, bool voteGiven)
    {
        serverRequesting.ReceiveVoteResponseFrom(this, requestedVoteCurrentTerm, voteGiven);
    }

    //todo check term in here
    public void ReceiveVoteResponseFrom(IServer server, int requestedVoteCurrentTerm, bool voteGiven)
    {
        if (voteGiven)
        { //potential bug: don't I need to also make sure that the vote is being 
            if (!VotesReceived.Contains(server))
            {
                VotesReceived.Add(server);
            }
        }
    }

    public void SendHeartbeatToAllNodes()
    {
        // Console.WriteLine("sending heartbeat to all nodes now");
        foreach (var server in OtherServersList)
        {
            if (this.LogBook.Count > 0)
            {
                server.ReceiveAppendEntriesLogFrom(this, [this.LogBook[0]]); //Todo: obviously this can't send the first entry every time
                //I think instead this could be this.logBook[serverNextIndex[server]] so we track the next log index for  that server.
            }
            server.ReceiveAppendEntriesLogFrom(this, 1, this.CurrentTerm);
        }
    }

    public void WinElection()
    {
        Console.WriteLine("Winning an election now");
        this.SendHeartbeatToAllNodes();
        this.State = States.Leader;
        //TODO: is this what we want to do? setting recognized leader to null??
        this.RecognizedLeader = null;

        //For log replication, we must adjust the nextIndex of each of our followers
        foreach (var server in OtherServersList) {
            this.NextIndex[server] = this.LogBook.Count + 1;
        };
    }

    public void StartBackgroundTaskToMonitorElectionTimeoutAndStartNewElection()
    {
        Stopwatch stopwatch = Stopwatch.StartNew(); //do I want the same stopwatch as the one I use to monitor if I haven't heard from the leader  yet?
        //Otherwise I worry that maybe we'll keep trying to start an election all over again
        while (this.State == States.Candidate)
        {
            if (stopwatch.ElapsedMilliseconds > this.ElectionTimeout)
            {
                StartElection();
                break;
            }
        }
    }

    public void StartBackgroundTaskToDetermineIfWeJustWonAnElection()
    {
        Console.WriteLine("Starting background thread to wait for news of winnign election");
        while(true)
        {
            if (State == States.Candidate)
            {
                if (haveMajorityOfVotes())
                {
                    WinElection();
                }
                else
                {
                    Thread.Sleep(1); //Wait just a milisecond to see if we get another vote
                }
                
            }
            else
            {
                break; //we aren't a candidate, so let the thread die. We'll start it again in StartElection();
                //Thread.Sleep(5); //Wait about 5 miliseconds then just check if we became a candidate or not.
            }
        }
    }

    private bool haveMajorityOfVotes()
    {
        //TODO: once I'm connfident this works, refactor to just return the if block.
        if (this.VotesReceived.Count >= (OtherServersList.Count / 2 + 1))
        {
            return true;
        }
        else
        {
            return false;
        }
        //if (this.OtherServersList.Count == 0)
        //{
        //    //We are the only node in the server!! return true (and also worry a lot)
        //    return true;
        //}
        //else
        //{
        //    //Will be implemented in more complicated test scenario. I think I'm going to need to use another thread to just say wait and keep checking everyone whose voted for us this term
        //    throw new NotImplementedException();
        //}
    }

    public void StartBackgroundTaskToMonitorTimeSinceHearingFromLeaderAndStartNewElection()
    {
        Console.WriteLine("Called StartBackgroundTaskToMonitorTimeSinceHearingFromLeader");
        //NOTE: this is for FOLLOWER state and for time since we received communication and has NOTHING to do with being a candidate in an election
       
        while (true)
        {
            //Hypothesis: Perhaps the 
            if (this.State == States.Follower && timeSinceHearingFromLeader.ElapsedMilliseconds > this.ElectionTimeout)
            {
                Console.WriteLine($"haven't heard from the leader in {this.timeSinceHearingFromLeader.ElapsedMilliseconds}, which is more than the election timeout of {this.ElectionTimeout}, starting election now");
                StartElection();
            }
            else
            {
                Thread.Sleep(1);
            }
        }
        // throw new NotImplementedException();
    }

    public void ListenForLeaderState()
    {
        //NOTE: this is for LEADER state
        while(true)
        {
            if (this.State == States.Leader)
            {
                ////EDGE case just in case
                if (!timeSinceLastSentHeartbeatAsLeader.IsRunning && !IsPaused)
                {
                    this.SendHeartbeatToAllNodes();
                    timeSinceLastSentHeartbeatAsLeader.Start();
                }
                else if (timeSinceLastSentHeartbeatAsLeader.ElapsedMilliseconds > IntervalAtWhichLeaderShouldSendHeartbeatsInMs)
                {
                    this.SendHeartbeatToAllNodes();
                    timeSinceLastSentHeartbeatAsLeader.Restart();
                }
                ////EDGE case just in case
                //if (timeSinceLastSentHeartbeatAsLeader.ElapsedMilliseconds > IntervalAtWhichLeaderShouldSendHeartbeatsInMs)
                //{
                //    this.SendHeartbeatToAllNodes();
                //    timeSinceLastSentHeartbeatAsLeader.Restart();
                //}
            }
            else
            {
                Thread.Sleep(20); //picked an arbitrary number greater than 1 but still not high in case we become the leader. But also the moment we become the leader
                //We send a heartbeat so I feel like we could wait 20 seconds to check 
                //I just don't want to be checking each second because the odds we become leader and don't send a heartbeat are -- well -- it would mean we have a bug
            }
        }
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, IEnumerable<RaftLogEntry> requests)
    {
        //TODO: Fix the method we're calling here (refactor following Jonathan's principles)
        this.RecognizedLeader = leader;
        if (requests == null)
        {
            ReceiveAppendEntriesLogFrom((Server)leader, requests.First().LogIndex, requests.First().TermNumber, requests.First());
        }
        else
        {
            foreach (var request in requests)
            {
                bool logIndexMatches = (request.PreviousLogIndex == this.LogBook.Count);
                bool termMatches = true;
                if (LogBook.Count - 1 < 0)
                {
                    termMatches = false;
                }
                else
                {
                    termMatches = request.PreviousLogTerm == this.LogBook[LogBook.Count - 1].TermNumber;
                }

                if (request.TermNumber < this.CurrentTerm)
                {
                    //reject the request because we have more info than it (its term is out of date)
                    this.SendAppendEntriesResponseTo(leader, request.LogIndex, false); //is request number my log index?? Or is this different?
                }
                // the previous term number and previous log index from the last request must match 
                else if (logIndexMatches && termMatches)
                {
                    //reject the request because we have more info than it (its term is out of date)
                    this.SendAppendEntriesResponseTo(leader, request.LogIndex, false); //is request number my log index?? Or is this different?
                }
                else
                {
                    this.LogBook.Add(request);

                    //this.SendAppendEntriesResponseTo(leader, requestNumber, true);
                    //I think I need to be putting this one in a different loop??
                    while (leader.HighestCommittedIndex > this.HighestCommittedIndex)
                    {
                        ApplyEntry(LogBook[HighestCommittedIndex + 1]); //THis is the line we broke on
                        IncrementHighestCommittedIndex();
                    }

                    this.SendAppendEntriesResponseTo(leader, request.LogIndex, true);
                    this.State = States.Follower;
                    this.timeSinceHearingFromLeader.Reset();
                    this.timeSinceHearingFromLeader.Start();
                }
            }
        }
    }

    public void ReceiveClientCommand((string, string) clientCommand)
    {
        //Compose a new command object
        RaftLogEntry request = new RaftLogEntry()
        {
            Command = clientCommand,
            LogIndex = 1, //update in the future
            TermNumber = this.CurrentTerm
        };

        //Put the object in the log (so later in a heartbeat it can be sent!)
        LogBook.Add(request);

        ////Pass it onto each child
        //foreach (var server in OtherServersList)
        //{
        //    server.ReceiveAppendEntriesLogFrom(this, request);
        //}
    }

    public void PauseTimeSinceHearingFromLeader()
    {
        this.timeSinceHearingFromLeader.Stop();
    }

    public void RestartTimeSinceHearingFromLeader()
    {
        this.timeSinceHearingFromLeader.Restart();
    }

    public void CommitEntry(int logIndex)
    {
        //Rachel next step of place to fix
        ApplyEntry(LogBook[logIndex]); //This is where it breaks, we're passing it the wrong log index
        IncrementHighestCommittedIndex();
    }

    public void IncrementHighestCommittedIndex()
    {
        this.HighestCommittedIndex++;
    }

    public void ApplyEntry(RaftLogEntry logEntry)
    {
        this.StateDictionary[logEntry.Command.Item1] = logEntry.Command.Item2;
    }

    public void PauseSimulation()
    {
        IsPaused = true;
        timeSinceLastSentHeartbeatAsLeader.Stop();
    }

    public void Resume()
    {
        IsPaused = false;
        timeSinceLastSentHeartbeatAsLeader.Restart();
    }
}
