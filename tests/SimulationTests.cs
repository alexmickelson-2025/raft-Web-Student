//using library;
//using NSubstitute;
//using Simulation;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace tests;

//public class SimulationTests
//{
//    //Testing a simulated network delay
//    //ONe potential hiccup I can see is I don't think I reset the list of people who have voted for them when they win an election
//    [Fact]
//    public void Test_simulated_network_delay_causes_message_not_received()
//    {
//        //Arrange
//        SimulationNode isLeader = new(false, false);
//        isLeader.State = States.Leader;
//        isLeader.CurrentTerm = 3;
//        var moq1 = Substitute.For<IServer>();
//        var moq2 = Substitute.For<IServer>();
//        isLeader.InnerNode.OtherServersList.Add(moq1);
//        isLeader.InnerNode.OtherServersList.Add(moq2);

        
//        //Act
//        //
//        isLeader.SimulatedNetworkDelay = 10000; //that's a delay of 10 seconds
//        isLeader.SendRequestForVoteRPCTo(moq1);
//        isLeader.SendRequestForVoteRPCTo(moq2);
//        Thread.Sleep(1);

//        //Assert
//        isLeader.moq1.ReceiveVoteRequestFrom(isLeader, 0, )

//    }
//}
