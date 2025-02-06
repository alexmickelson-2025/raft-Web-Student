using library;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NSubstitute;
using Simulation;

namespace tests;

public class ServerElectionTests
{
    ////Unit test 1
    //[Fact]
    //public async Task TestElectionTimeout()
    //{
    //    //Given the Election Timeout is 150 miliseconds
    //    //and Server L is the leader
    //    Server serverL = new Server();
    //    serverL.State = States.Leader;
    //    Server serverA = new Server();
    //    serverA.ElectionTimeout = 150;
    //    serverA.RecognizedLeader = serverL;

    //    // When Server A receives an Append entry RPC from the leader in 30 mili seconds
    //    await serverA.ProcessReceivedAppendEntryAsync(serverL, 30);

    //    //Then Server L remains the leader
    //    Assert.True(serverL.State == States.Leader);
    //}

    /********************************** *****************/

    [Fact]
    public void Testing_Why_Leader_Not_Elected_Bug_fix_EnsureFollowersReceiveRequestForVote()
    {
        //Arrange
        Server willBecomeLeader = new Server();
        willBecomeLeader.CurrentTerm = 2;
        var follower = Substitute.For<IServer>();
        willBecomeLeader.OtherServersList.Add(follower);

        //act
        willBecomeLeader.StartElection();

        //Assert
        follower.Received(1).ReceiveVoteRequestFrom(willBecomeLeader, 3);

    }

    //Testing #3
    //When a new node is initialized, it should be in follower state.
    [Fact]
    public void WhenNodeIsInitialized_ItIsInFollowerState()
    {
        //Arrange
        //Act (initialize Node)
        Server testServer = new Server();

        //Assert (test passes)
        Assert.Equal(States.Follower, testServer.State);
    }

    //Testing #5 
    // When the election time is reset, it is a random value between 150 and 300ms.
    [Fact]
    public void WhenElectionTimeReset_ItIsBetween150And300ms()
    {
        //Arrange
        Server testServer = new Server();

        //Act (reset election time)
        testServer.ResetElectionTimeout();

        //Assert
        Assert.True((testServer.ElectionTimeout <= 300) && (testServer.ElectionTimeout >= 150));
    }

    //still testing #5  (see above test as well)
    [Fact]
    public void WhenElectionTimeReset_ValueIsTrulyRandom()
    {
        //Arrange
        Server testServer = new();
        Dictionary<int, int> countAppears = new();

        //Act
        for (int i = 0; i < 100; i++)
        {
            testServer.ResetElectionTimeout();
            if (countAppears.ContainsKey(testServer.ElectionTimeout))
            {
                countAppears[testServer.ElectionTimeout] = countAppears[testServer.ElectionTimeout] + 1;
            }
            else
            {
                countAppears.Add(testServer.ElectionTimeout, 1);
            }
        }

        //Assert
        int duplicatesCount = 0;
        foreach (var countPair in countAppears)
        {
            if (countPair.Value > 1)
            {
                duplicatesCount++;
            }
        }
        Console.WriteLine("Duplicates count is ", duplicatesCount);
        //If the number of duplicates is less than  a particular percentage of the times we ran it
        //Then I will assume it is truly random
        Assert.True(duplicatesCount < 34); //Chat gpt shows math for probaility of duplicates being 33%, to me it makes sense because 100/150 = 66%
    }

    //Testing #2
    //When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
    [Fact]
    public void WhenNodeReceivesAppendEntries_ItKnowsTheNodeIsALeader()
    {
        //Arrange
        Server leader = new();
        Server follower = new();
        var entry = new RaftLogEntry
        {
            PreviousLogIndex = -1,
            TermNumber = 0
        };
        leader.LogBook.Add(entry);

        
        //Act
        //leader.SendAppendEntriesLogTo(follower); //I'm realizing that nowhere in my server code actually references this. so maybe I should test a different way
        follower.ReceiveAppendEntriesLogFrom(leader, entry);

        //Assert
        Assert.Equal(leader, follower.RecognizedLeader);
    }

    //Testing #17
    //When a follower node receives an AppendEntries request, it sends a response.
    [Fact]
    public void WhenReceivesAppendEntries_SendsResponse()
    {
        //Arrange
        Server follower = new();
        Server leader = new();
        follower.CurrentTerm = 1;

        //Act
        //i could keep track of the request id that it is rejecting or receiving so that when I need to know which one they're responding to I can tell
        //give my server an id too
        follower.ReceiveAppendEntriesLogFrom(leader, 1, 1); //request id 1

        //Assert
        Assert.Contains(leader, leader.AppendEntriesResponseLog[1]);
    }

    //Testing #18
    //Given a candidate receives an AppendEntries from a previous term, then rejects.
    [Fact]
    public void WhenReceivesAppendEntriesFromPreviousTerm_ThenRejects()
    {
        //Arrange
        Server follower = new();
        var leader = Substitute.For<IServer>();
        follower.CurrentTerm = 2;

        //Act
        //give my server an id too
        follower.ReceiveAppendEntriesLogFrom(leader, 1, 1);

        //Assert
        leader.Received(1).ReceiveAppendEntriesLogResponseFrom(follower, Arg.Is<AppendEntryResponse>(r => r.Accepted.Equals(false)));
    }

    //Testing #7
    //When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
    [Fact]
    public void WhenReceivesAppendEntries_ResetsElectionTimer()
    {
        //Arrange
        Server follower = new();
        follower.CurrentTerm = 13; //I just need it to be less than or equal to the term the request is sent on so we don't reject the request and not reset the timer.
        follower.timeSinceHearingFromLeader.Start();
        //Server leader = new();
        var leader = Substitute.For<Server>();
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 0 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 1 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 2 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 3 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 4 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 5 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 6 });
        follower.LogBook.Add(new RaftLogEntry { LogIndex = 7 });
        //Act
        Thread.Sleep(301); //because normally by 300 ms this would for sure start an election, but here we need to reset the election timer when we receive the request.
        follower.ReceiveAppendEntriesLogFrom(leader, 7, 15);

        //Assert
        //I do less than 40 because it may take some time to reset it but it certainly won't be 300 like it was before
        var time = follower.timeSinceHearingFromLeader.ElapsedMilliseconds;
        Assert.True(follower.timeSinceHearingFromLeader.ElapsedMilliseconds >= 0 && follower.timeSinceHearingFromLeader.ElapsedMilliseconds < 40);
        //Idea for the future we could also ensure the reset timer function was called.
    }

    //Testing #12
    //Given a candidate, when it receives an AppendEntries message from a node with a later term, then candidate loses and becomes a follower.
    [Fact]
    public void WhenCandidateReceivesAppendEntriesMessageWithLaterTerm_ThenRevertsToFollowerState()
    {
        //Arrange
        Server candidate = new();
        candidate.CurrentTerm = 2;
        candidate.State = States.Candidate;

        var newLeader = Substitute.For<IServer>();
        newLeader.CurrentTerm = 13;

        //Act
        //newLeader.SendAppendEntriesLogTo(candidate);
        candidate.ReceiveAppendEntriesLogFrom(newLeader, new RaftLogEntry
        {
            TermNumber = 13,
            PreviousLogIndex = -1
        });

        //Assert
        Assert.Equal(States.Follower, candidate.State);
    }

    //Testing #11
    //Given a candidate server that just became a candidate, it votes for itself.
    [Fact]
    public void WhenServerBecomesCandidate_ThenVotesForItself()
    {
        //Arrange
        Server willBeCandidate = new();

        //Act
        willBeCandidate.StartElection();

        //Assert
        //I could check that its vote is written to a votes list
        Assert.Contains(willBeCandidate, willBeCandidate.VotesReceived);
    }

    //Testing #10
    //A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
    //NOTE: Sometimes this fails and sometimes this passes depending on which time I run it, but now that I've added thread.sleep it seems to work every time
    [Fact]
    public void IfHaveNotYetVoted_VoteForALaterTermRequest()
    {
        //Arrange
        Server willSayYes = new();
        willSayYes.CurrentTerm = 1;
        Server inLaterTerm = new();
        inLaterTerm.CurrentTerm = 2;
        willSayYes.VotesCast = new Dictionary<int, Server>(); //Perhaps I can create a CastVotes property, int = term number, Server = server voted for

        //Act
        inLaterTerm.SendRequestForVoteRPCTo(willSayYes);
        Thread.Sleep(5); //to give it enough time to finish the background thread

        //Assert
        Assert.Contains(willSayYes, inLaterTerm.VotesReceived);
    }

    //Testing #10
    //A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
    [Fact]
    public void IfHaveNotYetVoted_StillDontVoteForAnEarlierTermRequest()
    {
        //Arrange
        Server willSayYes = new();
        willSayYes.CurrentTerm = 1;
        Server inLaterTerm = new();
        inLaterTerm.CurrentTerm = 0;
        willSayYes.VotesCast = new Dictionary<int, Server>(); //Perhaps I can create a CastVotes property, int = term number, Server = server voted for

        //Act
        inLaterTerm.SendRequestForVoteRPCTo(willSayYes);

        //Assert
        Assert.DoesNotContain(willSayYes, inLaterTerm.VotesReceived);
    }

    //Testing #14
    //If a node receives a second request for vote for the same term, it should respond no. (again, separate RPC for response)
    [Fact]
    public void IfAlreadyVoted_DontVoteThatTermAgain()
    {
        //Arrange
        Server someRandomServerWeVotedForInThatTerm = new();
        Server willSayNo = new();
        willSayNo.CurrentTerm = 1;
        willSayNo.VotesCast.Add(2, someRandomServerWeVotedForInThatTerm);
        Server inLaterTerm = new();
        inLaterTerm.CurrentTerm = 2;

        //Act
        inLaterTerm.SendRequestForVoteRPCTo(willSayNo); //I may also have a bug where I think I append the wrong "vote term" to the log

        //Assert
        Assert.DoesNotContain(willSayNo, inLaterTerm.VotesReceived);
    }

    //Testing #15
    //If a node receives a second request for vote for a future term, it should vote for that node.
    [Fact]
    public void IfHaveAlreadyVoted_VoteForALaterTermRequestAgain()
    {
        //TODO: failed onctwicee, item not found in collection

        //Arrange
        Server willSayYes = new();
        willSayYes.CurrentTerm = 1;
        Server inLaterTerm = new();
        inLaterTerm.CurrentTerm = 2;
        willSayYes.VotesCast = new Dictionary<int, Server>(); //Perhaps I can create a CastVotes property, int = term number, Server = server voted for

        Server evenLaterTerm = new();
        evenLaterTerm.CurrentTerm = 3;

        //Act
        inLaterTerm.SendRequestForVoteRPCTo(willSayYes);
        evenLaterTerm.SendRequestForVoteRPCTo(willSayYes);
        Thread.Sleep(5); //just to give it all time to process

        //Assert
        Assert.Contains(willSayYes, inLaterTerm.VotesReceived);
        Assert.Contains(willSayYes, evenLaterTerm.VotesReceived);
    }

    //Testing #19
    //When a candidate wins an election, it immediately sends a heart beat
    [Fact]
    public void UponWinningElection_ThenLeaderSendsHeartbeatEmptyAppendEntriesRPC()
    {
        //Arrange
        Server winningElection = new();
        winningElection.CurrentTerm = 1;
        var receivesHeartbeat = Substitute.For<IServer>();
        winningElection.OtherServersList = new List<IServer>() { receivesHeartbeat };

        //Act
        winningElection.WinElection();

        //Assert
        receivesHeartbeat.Received(1).ReceiveAppendEntriesLogFrom(winningElection, 1, 1);
    }

    //Testing #16
    //Given a candidate, when an election timer expires inside of an election, a new election is started.
    //TODO: Ask Alex or Adam how to make this test better so it's not just testing other features involved in an election timeout.
    [Fact]
    public void WhenElectionTimeoutOccurs_NewElectionStarts()
    {
        //Arrange
        //var isCandidate = Substitute.For<IServer>();
        //isCandidate.CallBase = true;  // Allow the real method implementation to run
        //isCandidate.ResetElectionTimeout();
        Server candidate = new Server();
        candidate.CurrentTerm = 1;
        var alreadyVotedOne = Substitute.For<IServer>();
        var discard = Substitute.For<IServer>();
        //alreadyVotedOne.VotesCast.Add(2, discard);
        //discard.VotesCast.Add(2, alreadyVotedOne);  //at this point we've set up a tie. each of them voted for each other in term 2 so when candidate tries to start an election it wont get the votes
        candidate.OtherServersList = new List<IServer> { discard, alreadyVotedOne };
        candidate.ResetElectionTimeout();

        //Act
        candidate.StartElection(); //now its current term will be 2, so it will not get the votes.
        var sleepTime = candidate.ElectionTimeout; //Sleep time is 0, the election timeout should not be 0!
        Thread.Sleep(sleepTime + 50); //so the election timeout will have just expired but we won't have finished the next one.

        //Assert
        Assert.Equal(3, candidate.CurrentTerm); //Logically current term should be 3 but that's per our implementation, we know it had better be greater than 2 though if it started a new one
        Assert.Equal(States.Candidate, candidate.State);
        Assert.Contains(candidate, candidate.VotesReceived); //Because it should have voted for itself that new election
    }

    //Testing #16 (but in reverse, if the election timer has not expired yet, don't start a new election!!
    ////Given a candidate, when an election timer expires inside of an election, a new election is started.
    ////TODO: Ask Alex or Adam how to make this test better so it's not just testing other features involved in an election timeout.
    ////NOTE: i feel a little bit more iffy about this one. Is it really thought through the way I want?
    //[Fact]
    //public void WhenElectionTimeoutHasNOTOccurred_NewElectionNOTYetBegun()
    //{
    //    //Arrange
    //    //var isCandidate = Substitute.For<IServer>();
    //    //isCandidate.CallBase = true;  // Allow the real method implementation to run
    //    //isCandidate.ResetElectionTimeout();
    //    Server candidate = new Server();
    //    candidate.CurrentTerm = 1;
    //    Server alreadyVotedOne = new();
    //    Server discard = new();
    //    alreadyVotedOne.VotesCast.Add(2, discard);
    //    discard.VotesCast.Add(2, alreadyVotedOne);  //at this point we've set up a tie. each of them voted for each other in term 2 so when candidate tries to start an election it wont get the votes
    //    candidate.OtherServersList = new List<IServer> { discard, alreadyVotedOne };

    //    candidate.ElectionTimeout = 150;

    //    //Act
    //    candidate.StartElection(); //now its current term will be 2, so it will not get the votes.
    //    var sleepTime = candidate.ElectionTimeout;
    //    Thread.Sleep(sleepTime  - 100); //so the election timeout will have not yet expired

    //    //Assert
    //    Assert.Equal(States.Candidate, candidate.State);
    //    Assert.Equal(2, candidate.CurrentTerm); //Logically current term should be 3 but that's per our implementation, we know it had better be greater than 2 though if it started a new one
    //    Assert.Contains(candidate, candidate.VotesReceived); //Because it should have voted for itself that new election
    //}

    //Testing #6
    //When a new election begins, the term is incremented by 1.
    [Fact]
    public void WhenNewElectionBegins_ThenTermImcremented()
    {
        //Arrange
        bool TrackTimeSinceHearingFromLeader = true;
        bool TrackTimeAtWhichLeaderShouldSendHeartbeats = false;
        Server server = new(TrackTimeSinceHearingFromLeader, TrackTimeAtWhichLeaderShouldSendHeartbeats);
        server.CurrentTerm = 10; //why is votess received 3?? for terms not in this test?? there shouldn't be.

        //For debug purposes
        int electionTimeout = server.ElectionTimeout;

        //Act
        Thread.Sleep(301); //A new election should have begin by now, if not two
        //I can tell that my election has started, because I changed to candidate state (instead of follower) but I think once I'm in candidate state I need to break out of my "election loop" when I hear from the leader
        //So perhaps I need to incorporate the time heard from leader variable into that.
        int laterElectionTimeout = server.ElectionTimeout;

        //Assert
        Assert.True(server.CurrentTerm > 10);

        /******
        //TODO: Bugs/flaws/NotYetImplementeds this test reveals when I look at it in the debugger:
        * First, VotesReceived, I think, needs to be tied to a current term
        * Second, when I started an election, I didn't actually vote for myself
        * Third, when I started an election, I didn't reset the election timeout
        ****/
    }

    //Testing #1
    //When a leader is active it sends a heart beat within 50ms
    [Fact]
    public void WhenLeaderIsActive_SendsHeartbeatEvery50ms()
    {
        //Arrange
        bool TrackTimeSinceHearingFromLeader = false;
        bool TrackTimeAtWhichLeaderShouldSendHeartbeats = true;
        Server winningElection = new Server(TrackTimeSinceHearingFromLeader, TrackTimeAtWhichLeaderShouldSendHeartbeats);
        winningElection.State = States.Candidate;
        winningElection.CurrentTerm = 2;
        var receivesHeartbeat = Substitute.For<IServer>();
        winningElection.OtherServersList = new List<IServer>() { receivesHeartbeat };

        //Act
        winningElection.State = States.Leader;
        Thread.Sleep(50); //A heartbeat should have been sent at 50ms, so now we should see it

        //Assert
        //THIS FAILS HALF THE TIME because sometimes it receives 2 calls
        receivesHeartbeat.Received(1).ReceiveAppendEntriesLogFrom(winningElection, 1, 2);

    }

    //Testing #1, I think both parameters for constructor could (should) be true at the same time and still pass, let's find out
    //When a leader is active it sends a heart beat within 50ms
    [Fact]
    public void WhenLeaderIsActive_SendsHeartbeatEvery50ms_TryBothConstructors()
    {
        //Arrange
        bool TrackTimeSinceHearingFromLeader = true;
        bool TrackTimeAtWhichLeaderShouldSendHeartbeats = true;
        Server winningElection = new Server(TrackTimeSinceHearingFromLeader, TrackTimeAtWhichLeaderShouldSendHeartbeats);
        winningElection.State = States.Candidate;
        winningElection.CurrentTerm = 2;
        var receivesHeartbeat = Substitute.For<IServer>();
        winningElection.OtherServersList = new List<IServer>() { receivesHeartbeat };

        //Act
        winningElection.State = States.Leader;
        Thread.Sleep(51); //A heartbeat should have been sent at 50ms, so now we should see it

        //Assert
        receivesHeartbeat.Received(1).ReceiveAppendEntriesLogFrom(winningElection, 1, 2);

    }

    //Testing #13
    // Given a candidate, when it receives an AppendEntries message from a node with an equal term, then candidate loses and becomes a follower.
    [Fact]
    public void WhenCandidateReceivesAppendEntriesMessage_AndNodeHasEqualTerm_ThenCandidateLosesElectionAndBecomesFollower()
    {
        //Arrange
        Server willLose = new();
        willLose.CurrentTerm = 3;
        willLose.State = States.Candidate;
        Server alreadyLeader = new();
        alreadyLeader.CurrentTerm = 3;
        alreadyLeader.LogBook.Add(new RaftLogEntry { PreviousLogTerm = 3, PreviousLogIndex = -1});
        alreadyLeader.LogBook.Add(new RaftLogEntry { PreviousLogTerm = 3, PreviousLogIndex = 0});
        alreadyLeader.LogBook.Add(new RaftLogEntry { PreviousLogTerm = 3, PreviousLogIndex = 1});

        //Act
        willLose.ReceiveAppendEntriesLogFrom(alreadyLeader, 2, alreadyLeader.CurrentTerm); //because the leader has the same term we must forfeit the election

        //Assert
        Assert.Equal(States.Follower, willLose.State);
    }

    //Testing #8
    //Given an election begins, when the candidate gets a majority of votes, it becomes a leader.
    [Fact]
    public void CandidateGetsMajorityOfVotes_AsOnlyServer_ThusBecomesLeader()
    {
        //Arrange
        Server onlyServerInCluser = new(true, true);
        onlyServerInCluser.State = States.Follower;
        onlyServerInCluser.OtherServersList = new List<IServer>();

        //Act
        onlyServerInCluser.StartElection();
        Thread.Sleep(15); //Not long enough for another election to start but ample time to ensure we finished voting for ourselves.

        //Assert
        Assert.Equal(States.Leader, onlyServerInCluser.State);
    }

    //Testing #4
    //When a follower doesn't get a message for 300ms then it starts an election.
    [Fact]
    public void WhenFollowerGetsNoMessageIn300ms_ThenStartsElection()
    {
        //Todo: this failed once. I think with adding that for each loop in the winElection function, it took more than 301 ms?
        //Arrange
        Server allByItself = new(true, true);
        allByItself.State = States.Follower; //this should be a given but just in case

        //Act
        Thread.Sleep(301); //thus it will get no message

        //Assert
        Assert.True(allByItself.State == States.Candidate || allByItself.State == States.Leader);
    }

    //Testing a simulated network delay
    //ONe potential hiccup I can see is I don't think I reset the list of people who have voted for them when they win an election
    //[Fact]
    //public void Test_simulated_network_delay_causes_message_not_received()
    //{
    //    //Arrange
    //    Server isLeader = new(false, false);
    //    isLeader.State = States.Leader;
    //    isLeader.CurrentTerm = 3;
    //    var moq1 = Substitute.For<IServer>();
    //    isLeader.OtherServersList.Add(moq1);


    //    //Act
    //    isLeader.NetworkDelay = 10000; //that's a delay of 10 seconds
    //    isLeader.SendRequestForVoteRPCTo(moq1);
    //    //Thread.Sleep(1);

    //    //Assert
    //    moq1.Received(0).ReceiveVoteRequestFrom(isLeader, 3);

    //}
}
