using Castle.Components.DictionaryAdapter.Xml;
using library;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    [Fact]
    public void WhenLeaderReceivesCommandFromClient_LeaderSendsLogEntryInNextAppendEntriesRPCToAllNodes()
    {
        //Questions to be decided so I can structure my logs appropriately:
        //What data types/structure do I want?
        //I need a term. For each term I have an index. So do I want a dictionary of arrays? (where the key in the dictionary is the term, adn the value is an array,
        //and the index of that array is the particular command? (eg command 0 is at index 0 to "add" and at index 1 is the command "add" and at index 2 we were told "decrement"

        //Arrange
        IServer leader = new Server();
        leader.CurrentTerm = 150;
        var follower1 = Substitute.For<IServer>();
        var follower2 = Substitute.For<IServer>();
        leader.OtherServersList = [follower1, follower2];

        //Act
        leader.ReceiveClientCommand("IncrementBy1");

        //Assert that each of the other servers received that log entry.
        //I can do that by being sure that it received a call to AppendEntryRPC and that call contained the command IncrementBy1.
        //The request they should be sending:
        RaftLogEntry request = new()
        {
            Command = "IncrementBy1",
            TermNumber = 150,
            LogIndex = 1,
        };
        follower1.Received(1).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(rle => rle.Command.Equals("IncrementBy1")));
        follower2.Received(1).ReceiveAppendEntriesLogFrom(leader, Arg.Is<RaftLogEntry>(le => le.Command.Equals("IncrementBy1")));
    }
}
