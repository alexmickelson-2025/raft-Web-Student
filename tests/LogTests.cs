using Castle.Components.DictionaryAdapter.Xml;
using library;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace tests;

//These are the tests for the log replicaiton scenarios assignment
public class LogTests
{
    //Example test case for what was number 6 on my list
    //Given a log entry
    //Then the log entry contains a state machine command
    //And the log entry contains the the term number at which the leader received it
    //And the log entry contains an integer index identifying its position in the log
    [Fact]
    public void SetupLogEntryFormat()
    {
        //Arrange
        RaftLogEntry logEntry = new();

        //Act
        //let's put in the information here
        logEntry.Command = ("someCommand", ""); //Do I really want this to be a string? For me I kind of want to make it a separate class?? But I'm not sure how to store even addition commands besides as string
        logEntry.TermNumber = 5;
        logEntry.LogIndex = 1;

        //Assert
        //These are kind of weak assert statements, but it's to test that these properties exist...
        Assert.Equal(("someCommand", ""), logEntry.Command);
        Assert.Equal(5, logEntry.TermNumber);
        Assert.Equal(1, logEntry.LogIndex);
    }

    //Testing Logs #3) when a node is new, its log is empty
    [Fact]
    public void NewNode_HasEmptyLog()
    {
        //Arrange
        IServer server;

        //Act
        server = new Server();

        //Assert
        Assert.Empty(server.LogBook);
    }

    //Testing Logs #1) when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
    //NOTE: Multiple tests for number 1 to get there.
    [Fact]
    public void WhenLeaderReceivesCommandFromClient_LeaderSendsLogEntryInNextAppendEntriesRPCToAllNodes()
    {
        //Arrange
        IServer leader = new Server(false, false);
        leader.State = States.Leader;
        //leader.PauseTimeSinceHearingFromLeader(); //because I don't want it to send a hearbeat right now
        leader.CurrentTerm = 150;

        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();
        leader.OtherServersList = [follower1, follower2];

        RaftLogEntry request = new()
        {
            Command = ("myKey", "IncrementBy1"),
            TermNumber = 150,
            LogIndex = 1,
        };

        //Act and assert
        leader.ReceiveClientCommand(("myKey", "IncrementBy1")); 

        //Step 1 of the assert: assert that if it's not time yet, we don't sent it!
        follower1.Received(0).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(rle => rle.Command.Item1.Equals("myKey"))]);
        follower1.Received(0).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(rle => rle.Command.Item2.Equals("IncrementBy1"))]);
        follower2.Received(0).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(le => le.Command.Item1.Equals("myKey"))]);
        follower2.Received(0).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(le => le.Command.Item2.Equals("IncrementBy1"))]);

        //Step 2 of the Act:
        //leader.RestartTimeSinceHearingFromLeader();
        leader.SendHeartbeatToAllNodes(); //normally this gets called every 50ms but that was causing problems with our heartbeat time being too fast, so I'll manually call it here and I'm turning heartbeats off.
        Thread.Sleep(50); //long enough for a heartbeat to go out

        //Step 2 of the Assert:
        follower1.Received(1).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(rle => rle.Command.Item1.Equals("myKey"))]);
        follower1.Received(1).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(rle => rle.Command.Item2.Equals("IncrementBy1"))]);
        follower2.Received(1).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(le => le.Command.Item1.Equals("myKey"))]);
        follower2.Received(1).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(le => le.Command.Item2.Equals("IncrementBy1"))]); //add an item2 in there
    }

    //Testing Logs #2) when a leader receives a command from the client, it is appended to its log
    [Fact]
    public void WhenClientSendsCommand_LeaderAppendsToItsLog()
    {
        //Arrange
        IServer leader = new Server();
        leader.LogBook = new(); //should be empty, but just to avoid any confusion

        //Act
        leader.ReceiveClientCommand(("myKey", "DecrementBy2"));

        //Assert
        var logEntry = leader.LogBook.SingleOrDefault(le => le.Command.Item1 == "myKey");
        Assert.NotNull(logEntry);  // Ensure the entry was added
        Assert.Single(leader.LogBook);
        Assert.Equal("myKey", logEntry.Command.Item1);
        Assert.Equal("DecrementBy2", logEntry.Command.Item2);
        //Assert.Contains(Arg.Is<RaftLogEntry>(le => le.Command.Equals("DecrementBy2")), leader.LogBook);
    }

    //Testing Logs #5) leaders maintain an "nextIndex" for each follower that is the index of the next log entry the leader will send to that follower
    [Fact]
    public void LeadersMaintainNextInex()
    {
        //Arrange
        IServer leader = new Server();
        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();

        //Act
        leader.OtherServersList = [follower1, follower2];
        leader.NextIndex[follower1] = 0;
        leader.NextIndex[follower2] = 1;

        //Assert
        Assert.Equal(0, leader.NextIndex[follower1]);
        Assert.Equal(1, leader.NextIndex[follower2]);
    }

    //Testing Logs #4) when a leader wins an election, it initializes the nextIndex for each follower to the index just after the last one in its log
    [Fact]
    public void WhenLeaderWinsElection_InitializesNextIndexForEachFollowerToIndexJustAfterLastOneInLog()
    {
        //Arrange
        IServer leader = new Server();
        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();
        leader.OtherServersList = [follower1, follower2];
        leader.NextIndex[follower1] = 1;
        leader.NextIndex[follower2] = 2;
        leader.LogBook = new List<RaftLogEntry>()
        {
            new(),
            new(),
            new(),
            new(),
            new()
        };

        //Act
        leader.WinElection();

        //Assert
        Assert.Equal(6, leader.NextIndex[follower1]);
        Assert.Equal(6, leader.NextIndex[follower2]);
    }

    //Testing Logs #6) Highest committed index from the leader is included in AppendEntries RPC's
    [Fact]
    public void SendingAppendEntryRPC_IncludesHighestCommittedIndex()
    {
        //Arrange
        //Arrange
        IServer leader = new Server();
        var follower1 = Substitute.For<IServer>();
        leader.OtherServersList = [follower1];
        leader.NextIndex[follower1] = 1;
        leader.LogBook = new List<RaftLogEntry>()
        {
            new(),
            new(),
            new(),
            new(),
            new()
        };
        leader.HighestCommittedIndex = 3;

        //Act
        leader.SendAppendEntriesLogTo(follower1);

        //Assert
        follower1.Received(1).ReceiveAppendEntriesLogFrom(leader, [Arg.Is<RaftLogEntry>(log => log.LeaderHighestCommittedIndex.Equals(3))]);
    }

    //Testing Logs #9) the leader commits logs by incrementing its committed log index
    [Fact]
    public void LeaderCommitsLog_ByIncrementingCommitedIndex()
    {
        //Arrange
        IServer leader = new Server();
        leader.HighestCommittedIndex = 2;

        //Act
        leader.IncrementHighestCommittedIndex(); //does leader need an index? Or can it always just increment?? Ask Adam or Alex

        //Assert
        Assert.Equal(3, leader.HighestCommittedIndex);
    }

    //Testing Logs #7) When a follower learns that a log entry is committed, it applies the entry to its local state machine
    [Fact]
    public void TestApplyEntry_ActuallyAppliesEntry()
    {
        //Note for the grader: This test is really a precursor for testing number 7.
        //What this test really does is tests my ApplyEntry() function  as it's a small part of test case number 7
        //and I have another test to test number 7 more completely (it just depends on ApplyEntry() working correctly, which is why I have a separate test)

        //Arrange
        IServer follower = new Server();
        RaftLogEntry logEntry = new RaftLogEntry()
        {
            Command = ("someKey", "someValue")
        };

        //Act
        follower.ApplyEntry(logEntry);

        //Assert
        Assert.Equal("someValue", follower.StateDictionary["someKey"]);
    }

    //Testing Logs #7) When a follower learns that a log entry is committed, it applies the entry to its local state machine
    [Fact]
    public void WhenFollowerLearnsLogEntryIsCommited_AppliesEntryToLocalStateMachine()
    {
        //Arrange
        IServer follower = new Server();
        follower.CurrentTerm = 1;
        var leader = Substitute.For<IServer>();
        leader.HighestCommittedIndex = 1;
        follower.HighestCommittedIndex = 0;
        var logEntry = new RaftLogEntry()
        {
            Command = ("LetterA", "Value5"),
            TermNumber = 1,
            LeaderHighestCommittedIndex = 1 //Question: do I really need this in the log entry?? Or can I just have it be on the leader?
        };
        follower.LogBook.Add(logEntry);

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, [logEntry]);

        //Assert
        Assert.Equal("Value5", follower.StateDictionary["LetterA"]);
    }

    //Testing Logs #7) When a follower learns that a log entry is committed, it applies the entry to its local state machine
    [Fact]
    public void WhenLogEntryNOTCommited_EntryNOTAppliedToLocalStateMachine()
    {
        //Arrange
        IServer follower = new Server();
        follower.CurrentTerm = 1;
        var leader = Substitute.For<IServer>();
        leader.HighestCommittedIndex = 0;
        var logEntry = new RaftLogEntry()
        {
            Command = ("LetterA", "Value5"),
            TermNumber = 1
        };
        follower.LogBook.Add(logEntry);

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, [logEntry]);

        //Assert
        Assert.Empty(follower.StateDictionary); //because since the highest committed index is 0, this should not have been applied.
    }

    //Testing #10) given a follower receives an appendentries with log(s) it will add those entries to its personal log
    [Fact]
    public void GivenFollowerReceivesAppendEntries_AndItContainsLogs_ThenAddsEntriesToPersonalLog()
    {
        //Arrange
        IServer follower = new Server();
        follower.LogBook = [];
        follower.CurrentTerm = 3;

        var leader = Substitute.For<IServer>();
        var entry1 = new RaftLogEntry()
        {
            Command = ("someKey", "5"),
            TermNumber = 3
        };
        var entry2 = new RaftLogEntry()
        {
            Command = ("someOtherKey", "7"),
            TermNumber = 3
        };
        var entries = new List<RaftLogEntry>() { entry1, entry2 };
        leader.LogBook = entries;

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, entries);

        //Assert
        Assert.Contains(entry1, follower.LogBook);
        Assert.Contains(entry2, follower.LogBook);
    }

    //Testing Logs #11) a followers response to an appendentries includes the followers term number and log entry index
    [Fact]
    public void FollowerResponseToAppendEntries_IncluesTermNumberAndLogIndex()
    {
        //Arrange
        IServer follower = new Server();
        var leader = Substitute.For<IServer>();
        //leader.CurrentTerm = 3; //NOTE: uncommenting this is all it takes for the test to pass, but I'm leaving it red intentionally because I want to refactor other functions so we don't 
        RaftLogEntry logEntry = new RaftLogEntry()
        {
            LogIndex = 10,
            TermNumber = 3
        };

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, [logEntry]);

        //Assert
        AppendEntryResponse expectedResponse = new()
        {
            TermNumber = 3,
            LogIndex = 10,
            Accepted = true,
        };

        leader.Received(1).ReceiveAppendEntriesLogResponseFrom(follower, Arg.Is<AppendEntryResponse>(reply => reply.LogIndex.Equals(10)));
        //The reason this one (term number) fails is because we currently get term number from the server, when really we need to get it from the request
        //But that's going to be a separate refactor of our ReceiveAppendEntiresLogFrom method/how it calls the receive response methos
        leader.Received(1).ReceiveAppendEntriesLogResponseFrom(follower, Arg.Is<AppendEntryResponse>(reply => reply.TermNumber.Equals(3))); 
    }

    //Testing Logs #8) when the leader has received a majority confirmation of a log, it commits it  
    [Fact]
    public void WhenLeaderReceivesMajorityConfirmation_ThenCommitsLog()
    {
        //Arrange
        IServer leader = new Server();
        leader.State = States.Candidate;
        leader.CurrentTerm = 5;
        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();
        var follower3 = Substitute.For<IServer>();
        var follower4 = Substitute.For<IServer>();
        var follower5 = Substitute.For<IServer>();
        leader.OtherServersList = new List<IServer>() { follower1, follower2, follower3, follower4, follower5};
        leader.HighestCommittedIndex = -1;

        AppendEntryResponse reply = new AppendEntryResponse()
        {
            LogIndex = 0,
            TermNumber = 5,
            Accepted = true,
        };
        var logEntry = new RaftLogEntry
        {
            Command = ("", ""),
            LeaderHighestCommittedIndex = leader.HighestCommittedIndex,
            LogIndex = 0,
            TermNumber = 5,
        };
        //leader.LogBook.Add(new RaftLogEntry { Command = ("Intentionally Empty", "INtentionallyEmpty") });
        leader.LogBook.Add(logEntry); //So we have an index for the log book.
        
        //Act
        leader.ReceiveAppendEntriesLogResponseFrom(follower1, reply);
        leader.ReceiveAppendEntriesLogResponseFrom(follower2, reply);
        leader.ReceiveAppendEntriesLogResponseFrom(follower5, reply);

        //Assert
        Assert.Equal(0, leader.HighestCommittedIndex);
    }

    //Testing Logs #13) given a leader node, when a log is committed, it applies it to its internal state machine
    [Fact]
    public void WhenLeaderCommitsLog_AppliesToInternalStateMachine()
    {
        //Arrange
        IServer server = new Server();
        var log = new RaftLogEntry
        {
            LogIndex = 11,
            TermNumber = 5,
            Command = ("Test", "5")
        };
        server.LogBook.Add(log);

        //Act
        server.CommitEntry(0);

        //Assert
        Assert.Equal("5", server.StateDictionary["Test"]);
    }

    //Testing Logs #16) when a leader sends a heartbeat with a log, but does not receive responses from a majority of nodes, the entry is uncommitted 
    [Fact]
    public void WhenLeaderNotYetHaveMajorityConfirmation_ThenLogNotcommittedYet()
    {
        //Arrange
        IServer leader = new Server();
        leader.State = States.Leader;
        leader.CurrentTerm = 5;
        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();
        var follower3 = Substitute.For<IServer>();
        var follower4 = Substitute.For<IServer>();
        var follower5 = Substitute.For<IServer>();
        leader.OtherServersList = new List<IServer>() { follower1, follower2, follower3, follower4, follower5 };
        leader.HighestCommittedIndex = -1;

        AppendEntryResponse PositiveReply = new AppendEntryResponse()
        {
            LogIndex = 0,
            TermNumber = 5,
            Accepted = true,
        };
        //AppendEntryResponse NegativeReply = new AppendEntryResponse()
        //{
        //    LogIndex = 0,
        //    TermNumber = 5,
        //    Accepted = true,
        //};
        var logEntry = new RaftLogEntry
        {
            Command = ("", ""),
            LeaderHighestCommittedIndex = leader.HighestCommittedIndex,
            LogIndex = 0,
            TermNumber = 5,
        };
        //leader.LogBook.Add(new RaftLogEntry { Command = ("Intentionally Empty", "INtentionallyEmpty") });
        leader.LogBook.Add(logEntry); //So we have an index fo rthe log book.

        //Act
        leader.SendAppendEntriesLogTo(follower1);
        leader.SendAppendEntriesLogTo(follower2);
        leader.SendAppendEntriesLogTo(follower3);
        leader.SendAppendEntriesLogTo(follower4);
        leader.SendAppendEntriesLogTo(follower5);

        //It even receives a response from some of them:
        leader.ReceiveAppendEntriesLogResponseFrom(follower1, PositiveReply);
        leader.ReceiveAppendEntriesLogResponseFrom(follower5, PositiveReply);

        //Assert
        Assert.Equal(-1, leader.HighestCommittedIndex);
    }

    //Testing Logs #14) when a follower receives a valid heartbeat, it increases its commitIndex to match the commit index of the heartbeat  
    //      - reject the heartbeat if the previous log index / term number does not match your log
    [Fact]
    public void WhenFolowerReceivesValidHeartbeat_IncreasesCommitIndex()
    {
        //Arrange
        IServer follower = new Server();
        follower.CurrentTerm = 1;
        follower.HighestCommittedIndex = -1; //because this will be its first committed index in the test
        var leader = Substitute.For<IServer>();
        RaftLogEntry logEntry = new()
        {
            LeaderHighestCommittedIndex = 0,
            TermNumber = 1,
        };

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, [logEntry]);

        //Assert
        Assert.Equal(0, follower.HighestCommittedIndex);
    }

    //Testing Logs # 14 part 2 
    //      - reject the heartbeat if the previous log index / term number does not match your log
    [Fact]
    public void RejectHeartbeatIfPreviousLogIndexAndTermNumberDoNotMatch()
    {
        //Arrange
        IServer leader = Substitute.For<IServer>();
        IServer follower = new Server();
        var someLogThatWontMatch = new RaftLogEntry()
        {
            LeaderHighestCommittedIndex = 3,
            PreviousLogIndex = 2,
            PreviousLogTerm = 2,
            TermNumber = 3,
        };

        var someOldLog = new RaftLogEntry()
        {
            LeaderHighestCommittedIndex = 3,
            LogIndex = 1,
            TermNumber = 2
        };

        follower.LogBook.Add(new RaftLogEntry { }); //so now we're at index 1 for the log book, and the next one it will get has previous index 2, so this isn't going to match
        follower.LogBook.Add(someOldLog);

        //Act
        follower.ReceiveAppendEntriesLogFrom(leader, [someLogThatWontMatch]);

        //Assert
        leader.Received().ReceiveAppendEntriesLogResponseFrom(follower, Arg.Is<AppendEntryResponse>(reply => reply.Accepted.Equals(false)));
    }

    //Testing Logs #15) When sending an AppendEntries RPC, the leader includes the index and term of the entry in its log that immediately precedes the new entries  
    //   - if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC
    [Fact]
    public void WhenSendingAppendEntriesRPC_IfRejected_DecrementRPC_andTryAgain()
    {
        //Arrange
        IServer leader = new Server();
        IServer follower = Substitute.For<IServer>();
        var firstLogWeCareAbout = new RaftLogEntry()
        {
            LeaderHighestCommittedIndex = 1,
            PreviousLogIndex = 1,
            PreviousLogTerm = 1,
            TermNumber = 2,
            LogIndex = 0,
        };
        var secondLog = new RaftLogEntry()
        {
            LeaderHighestCommittedIndex = 3,
            LogIndex = 1,
            PreviousLogIndex = 0,
            TermNumber = 2
        };

        var someLogThatWontMatch = new RaftLogEntry()
        {
            LeaderHighestCommittedIndex = 3,
            PreviousLogIndex = 1,
            PreviousLogTerm = 40, //see? clearly doesn't match
            TermNumber = 4,
            LogIndex = 2
        };

        leader.LogBook.Add(firstLogWeCareAbout);
        leader.LogBook.Add(secondLog);
        leader.LogBook.Add(someLogThatWontMatch);

        var rejectionResponse = new AppendEntryResponse
        {
            Accepted = false,
            LogIndex = 2
        };

        //Act
        //When this function is called, the leader not only rejects it, but the leader is going to re-try by sending the previous log
        //follower.ReceiveAppendEntriesLogFrom(leader, someLogThatWontMatch);
        leader.ReceiveAppendEntriesLogResponseFrom(follower, rejectionResponse);

        //Assert
        //but we will change this so it asserts the previous log entry was received.
        follower.Received().ReceiveAppendEntriesLogFrom(leader, [Arg.Is<IEnumerable<RaftLogEntry>>(log => log.LogIndex.Equals(1))]); 
        
        follower.Received().ReceiveAppendEntriesLogFrom(leader, [secondLog]); 
         
    }


    //[Fact]
    //public void WhenFollowerReceivesHeartbeat_AndPreviousIndexDoesNotMatch_ThenRejects()
    //{
    //    //Arrange
    //    IServer leader = new Server();
    //    leader.State = States.Candidate;
    //    leader.CurrentTerm = 5;
    //    leader.LogBook.Add(new RaftLogEntry { });
    //    leader.LogBook.Add(new RaftLogEntry { });
    //    var newLog = new RaftLogEntry
    //    {
    //        TermNumber = 5,
    //        LogIndex = 2
    //    };

    //    leader.LogBook.Add(newLog); // and we know its index is index 2

    //    var follower1 = Substitute.For<IServer>();
    //    follower1.LogBook.Add(new RaftLogEntry {
    //        TermNumber = 4
    //    });
    //    follower1.LogBook.Add(new RaftLogEntry {
    //        TermNumber = 4
    //    }); //so its index is 1



    //    //Act
    //    follower1.ReceiveAppendEntriesLogFrom(leader, [newLog]);

    //}
}
