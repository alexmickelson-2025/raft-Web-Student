using library;
using System.Security.Cryptography.X509Certificates;

namespace tests;

public class UnitTest1
{
    [Fact]
    public async Task TestElectionTimeout()
    {
        //Given the Election Timeout is 150 miliseconds
        //and Server L is the leader
        Server serverL = new Server();
        serverL.State = States.Leader;
        Server serverA = new Server();
        serverA.ElectionTimeout = 150;
        serverA.RecognizedLeader = serverL;

        // When Server A receives an Append entry RPC from the leader in 30 mili seconds
        await serverA.ProcessReceivedAppendEntryAsync(serverL, 30);

        //Then Server L remains the leader
        Assert.True(serverL.State == States.Leader);
    }
}
