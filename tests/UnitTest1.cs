using library;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

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

    //Testing # 3
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

    //Testing # 5 
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
        
    }
}
