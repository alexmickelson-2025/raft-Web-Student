using library;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NSubstitute;

namespace tests;

public class UnitTest1
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
    public void WhenElectionTimeReset_ItIsBetween150And300ms() {
        //Arrange
        Server testServer = new Server();

        //Act (reset election time)
        testServer.ResetElectionTimeout();

        //Assert
        Assert.True((testServer.ElectionTimeout <= 300) && (testServer.ElectionTimeout >= 150));
    }

    //still testing #5  (see above test as well)
    [Fact]
    public void WhenElectionTimeReset_ValueIsTrulyRandom() {
        //Arrange
        Server testServer = new();
        Dictionary<int,int> countAppears = new();

        //Act
        for (int i = 0; i < 100; i++) {
            testServer.ResetElectionTimeout();
            if (countAppears.ContainsKey(testServer.ElectionTimeout)) {
                countAppears[testServer.ElectionTimeout] = countAppears[testServer.ElectionTimeout] + 1;
            }
            else {
                countAppears.Add(testServer.ElectionTimeout, 1);
            }
        }

        //Assert
        int duplicatesCount = 0;
        foreach (var countPair in countAppears) {
            if (countPair.Value > 1) {
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

        //Act
        leader.SendAppendEntriesLogTo(follower);

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
        follower.ReceiveAppendEntriesLogFrom(leader, 1, 1); //reequest id 1

        //Assert
        Assert.True(leader.AppendEntriesResponseLog[1]);
    }

    //Testing #18
    //Given a candidate receives an AppendEntries from a previous term, then rejects.
    [Fact]
    public void WhenReceivesAppendEntriesFromPreviousTerm_ThenRejects()
    {
        //Arrange
        Server follower = new ();
        Server leader = new();
        follower.CurrentTerm = 2;

        //Act
        //give my server an id too
        follower.ReceiveAppendEntriesLogFrom(leader, 1, 1); //request id 1

        //Assert
        Assert.False(leader.AppendEntriesResponseLog[1]);
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
        Server leader = new();

        //Act
        Thread.Sleep(301); //because normally by 300 ms this would for sure start an election, but here we need to reset the election timer when we receive the request.
        follower.ReceiveAppendEntriesLogFrom(leader, 15, 15);

        //Assert
        //I do less than 40 because it may take some time to reset it but it certainly won't be 300 like it was before
        var time = follower.timeSinceHearingFromLeader.ElapsedMilliseconds;
        Assert.True(follower.timeSinceHearingFromLeader.ElapsedMilliseconds >= 0 && follower.timeSinceHearingFromLeader.ElapsedMilliseconds < 40); 
        //Idea for the future we could also ensure the reset timer function was called.
        //I could also wrap the stopwatch in a class that would be easier to mock out. I think that actually would be a better way to test it??
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

        Server newLeader = new();
        newLeader.CurrentTerm = 13;

        //Act
        newLeader.SendAppendEntriesLogTo(candidate);

        //Assert
        Assert.Equal(States.Follower, candidate.State);
    }

    //Testing #11
    //Given a candidate server that just became a candidate, it votes for itself.
    [Fact]
    public void WhenServerBecomesCandidate_ThenVotesForItself() {
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
    [Fact]
    public void IfHaveNotYetVoted_VoteForALaterTermRequest() {
        //Arrange
        Server willSayYes = new();
        willSayYes.CurrentTerm = 1;
        Server inLaterTerm = new();
        inLaterTerm.CurrentTerm = 2;
        willSayYes.VotesCast = new Dictionary<int, Server>(); //Perhaps I can create a CastVotes property, int = term number, Server = server voted for

        //Act
        inLaterTerm.SendRequestForVoteRPCTo(willSayYes);

        //Assert
        Assert.Contains(willSayYes, inLaterTerm.VotesReceived);
    }

    //Testing #10
    //A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
    [Fact]
    public void IfHaveNotYetVoted_StillDontVoteForAnEarlierTermRequest() {
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
    public void IfAlreadyVoted_DontVoteThatTermAgain() {
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
        winningElection.OtherServersList = new List<IServer>() { receivesHeartbeat};

        //Act
        winningElection.WinElection();

        //Assert
        receivesHeartbeat.Received(1).ReceiveAppendEntriesLogFrom(winningElection, 1, 1);
    }

    //Testing #16
    //Given a candidate, when an election timer expires inside of an election, a new election is started.
    //TODO: Ask Alex or Adam how to make this test better so it's not just testing other features involved in an election timeout.
    //[Fact]
    //public void WhenElectionTimeoutOccurs_NewElectionStarts()
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
    //    candidate.OtherServersList = new List<IServer> { discard , alreadyVotedOne};

    //    //Act
    //    candidate.StartElection(); //now its current term will be 2, so it will not get the votes.
    //    var sleepTime = candidate.ElectionTimeout;
    //    Thread.Sleep(sleepTime + 50); //so the election timeout will have just expired but we won't have finished the next one.

    //    //Assert
    //    Assert.Equal(States.Candidate, candidate.State);
    //    Assert.True(candidate.CurrentTerm > 2); //Logically current term should be 3 but that's per our implementation, we know it had better be greater than 2 though if it started a new one
    //    Assert.Contains(candidate, candidate.VotesReceived); //Because it should have voted for itself that new election
    //}

    //Testing #6
    //When a new election begins, the term is incremented by 1.
    [Fact]
    public void WhenNewElectionBegins_ThenTermImcremented()
    {
        //Arrange
        bool TrackTimeSinceHearingFromLeader = true;
        Server server = new(TrackTimeSinceHearingFromLeader);
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
        //TODO: Bugs/flaws this test reveals when I look at it in the debugger:
        * First, VotesReceived, I think, needs to be tied to a current term
        * Second, when I started an election, I didn't actually vote for myself
        * Third, when I started an election, I didn't reset the election timeout
        ****/
    }
}
