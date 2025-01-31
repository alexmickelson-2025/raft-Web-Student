using library;
using NSubstitute;
using Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests;

public class PauseNodesInClassTests
{
    [Fact]
    public void CanPauseNode()
    {
        //Arrange
        Server server = new Server(true, true); //so it has all functionality
        server.State = States.Leader;
        IServer follower1 = Substitute.For<IServer>();
        IServer follower2 = Substitute.For<IServer>();
        IServer follower3 = Substitute.For<IServer>();
        server.OtherServersList.Add(follower1);
        server.OtherServersList.Add(follower2);
        server.OtherServersList.Add(follower3);
        //IServer simulation = new SimulationNode(server);

        //Act
        server.PauseSimulation(); //My thought in here is that I will call my pauseElectionTimeout function, and later call the reset.
        server.CurrentTerm = 10;
        Thread.Sleep(400); //wait 400ms to ensure no heartbeat was sent

        //Assert
        follower1.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower2.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower3.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
    }

    //Test case 2
    [Fact]
    public void CanPauseNodeThenResumeIn400Milliseconds()
    {
        //Arrange
        IServer server = new Server(true, true); //so it has all functionality
        server.State = States.Leader;
        IServer follower1 = Substitute.For<IServer>();
        IServer follower2 = Substitute.For<IServer>();
        IServer follower3 = Substitute.For<IServer>();
        server.OtherServersList.Add(follower1);
        server.OtherServersList.Add(follower2);
        server.OtherServersList.Add(follower3);
        //IServer simulation = new SimulationNode(server);

        //Act
        server.PauseSimulation(); //My thought in here is that I will call my pauseElectionTimeout function, and later call the reset.
        server.CurrentTerm = 10;
        Thread.Sleep(400); //wait 400ms to ensure no heartbeat was sent

        //Assert it did not receive a heartbeat after 400ms
        follower1.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower2.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower3.Received(0).ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());

        //Act again
        server.Resume();
        Thread.Sleep(400);

        //Assert
        follower1.Received().ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower2.Received().ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
        follower3.Received().ReceiveAppendEntriesLogFrom(server, Arg.Any<int>(), Arg.Any<int>());
    }

    //Test Case 3 When follower gets paused, it does not time out to become a candidate
    [Fact]
    public void WhenFollowerGetsPaused_DoesNotTimeOutToBecomeCandidate()
    {
        //Arrange

        //Act

        //Assert
        Assert.Equal(1, 0);
    }
}
