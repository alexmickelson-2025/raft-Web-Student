using Castle.Components.DictionaryAdapter.Xml;
using library;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
        logEntry.Command = "someCommand"; //Do I really want this to be a string? For me I kind of want to make it a separate class?? But I'm not sure how to store even addition commands besides as string
        logEntry.TermNumber = 5;
        logEntry.LogIndex = 1;

        //Assert
        //These are kind of weak assert statements, but it's to test that these properties exist...
        Assert.Equal("someCommand", logEntry.Command);
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
        //Questions to be decided so I can structure my logs appropriately:
        //What data types/structure do I want?
        //I need a term. For each term I have an index. So do I want a dictionary of arrays? (where the key in the dictionary is the term, adn the value is an array,
        //and the index of that array is the particular command? (eg command 0 is at index 0 to "add" and at index 1 is the command "add" and at index 2 we were told "decrement"

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
            Command = "IncrementBy1",
            TermNumber = 150,
            LogIndex = 1,
        };

        //Act and assert
        leader.ReceiveClientCommand("IncrementBy1"); 

        //Step 1 of the assert: assert that if it's not time yet, we don't sent it!
        follower1.Received(0).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(rle => rle.Command.Equals("IncrementBy1")));
        follower2.Received(0).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(le => le.Command.Equals("IncrementBy1")));

        //Step 2 of the Act:
        //leader.RestartTimeSinceHearingFromLeader();
        leader.SendHeartbeatToAllNodes(); //normally this gets called every 50ms but that was causing problems with our heartbeat time being too fast, so I'll manually call it here and I'm turning heartbeats off.
        Thread.Sleep(50); //long enough for a heartbeat to go out

        //Step 2 of the Assert:
        follower1.Received(1).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(rle => rle.Command.Equals("IncrementBy1")));
        follower2.Received(1).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(le => le.Command.Equals("IncrementBy1")));
    }

    //Testing Logs #2) when a leader receives a command from the client, it is appended to its log
    [Fact]
    public void WhenClientSendsCommand_LeaderAppendsToItsLog()
    {
        //Arrange
        IServer leader = new Server();
        leader.LogBook = new(); //should be empty, but just to avoid any confusion

        //Act
        leader.ReceiveClientCommand("DecrementBy2");

        //Assert
        var logEntry = leader.LogBook.SingleOrDefault(le => le.Command == "DecrementBy2");
        Assert.NotNull(logEntry);  // Ensure the entry was added
        Assert.Single(leader.LogBook);
        Assert.Equal("DecrementBy2", logEntry.Command);
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
}
