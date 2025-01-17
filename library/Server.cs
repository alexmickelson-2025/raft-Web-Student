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
    public States State = States.Follower;
    public int ElectionTimeout { get; set; } //Specifies the Election Timeout in milisecondss
    public Server? RecognizedLeader { get; set; }

    public Dictionary<int, bool> AppendEntriesResponseLog = new();
    public int CurrentTerm { get; set; }
    public List<IServer> OtherServersList = new();

    public Dictionary<int, Server> VotesCast = new(); //<termNumber, ServerWeVotedFor>

    public List<Server> VotesReceived = new(); //If it has a Server in it, that means the server has voted for it with that current term.

    public Stopwatch timeSinceHearingFromLeader = new();

    public Server()
    {

    }

    public Server(bool TrackTimeSinceHearingFromLeaderAndStartElectionBecauseOfIt)
    {
        if (TrackTimeSinceHearingFromLeaderAndStartElectionBecauseOfIt) {
            this.ResetElectionTimeout();
            this.timeSinceHearingFromLeader.Start();
            new Thread(() => StartBackgroundTaskToMonitorTimeSinceHearingFromLeaderAndStartNewElection()) { IsBackground = true }.Start();
        }
    }

    public void ResetElectionTimeout()
    {
        int val = (Random.Shared.Next() % 150) + 150;
        ElectionTimeout = val;
    }

    public void SendAppendEntriesLogTo(Server follower)
    {
        follower.ReceiveAppendEntriesLogFrom(this, 0, this.CurrentTerm); //I need to be able to automatically increment this
    }

    public void ReceiveAppendEntriesLogFrom(Server server, int requestNumber, int requestCurrentTerm)
    {
        this.RecognizedLeader = server;
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

    private void SendAppendEntriesResponseTo(Server server, int requestNumber, bool accepted)
    {
        server.ReceiveAppendEntriesLogResponseFrom(this, requestNumber, accepted);
    }

    public void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool accepted)
    {

        if (!AppendEntriesResponseLog.ContainsKey(requestNumber))
        {
            AppendEntriesResponseLog.Add(requestNumber, accepted);
        }
    }

    public void StartElection()
    {
        this.VotesReceived.Add(this);
        this.State = States.Candidate;
        this.CurrentTerm++;
        //syntax from this stack overflow article https://stackoverflow.com/questions/4161120/how-should-i-create-a-background-thread
        //new Thread(() => NameOfYourMethod()) { IsBackground = true }.Start();
        new Thread(() => StartBackgroundTaskToMonitorElectionTimeoutAndStartNewElection()) { IsBackground = true }.Start();
    }

    public void SendRequestForVoteRPCTo(Server server)
    {
        server.ReceiveVoteRequestFrom(this, this.CurrentTerm);
    }

    //The reason we pass the requestedVote current term (even though it's a property on the server requesting) is the server requesting might update its term after we receive it, so we can't trust that property and must specify it
    private void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
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

    private void ReceiveVoteResponseFrom(Server server, int requestedVoteCurrentTerm, bool voteGiven)
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
        foreach (var server in OtherServersList)
        {
            server.ReceiveAppendEntriesLogFrom(this, 1, this.CurrentTerm);
        }
    }

    public void WinElection()
    {
        this.SendHeartbeatToAllNodes();
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
       // throw new NotImplementedException();
    }

    public void StartBackgroundTaskToMonitorTimeSinceHearingFromLeaderAndStartNewElection()
    {
        //NOTE: this is for FOLLOWER state and for time since we received communication and has NOTHING to do with being a candidate in an election
       
        //do I want the same stopwatch as the one I use to monitor if I haven't heard from the leader  yet? But that's what resetting the state when I get a message from the leader does.
        //Otherwise I worry that maybe we'll keep trying to start an election all over again
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

    // public async Task ProcessReceivedAppendEntryAsync(Server fromServer, int MilisecondsAtWhichReceived)
    // {
    //     await Task.CompletedTask;
    //     return;
    // }
}
