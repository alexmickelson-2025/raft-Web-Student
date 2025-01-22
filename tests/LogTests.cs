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
}
