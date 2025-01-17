﻿using library;
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

        //
    }
}
