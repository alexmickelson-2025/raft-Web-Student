using System.Runtime.CompilerServices;

namespace library;

public enum States
{
    Follower,
    Candidate,
    Leader
};

public class Server
{
    public States State = States.Follower; 
    public int ElectionTimeout { get; set; } //Specifies the Election Timeout in milisecondss
    public Server? RecognizedLeader {get;set;}

    public void ResetElectionTimeout()
    {
        ElectionTimeout = 200;
        throw new NotImplementedException(); //because it needs to be randomly generated
    }

    // public async Task ProcessReceivedAppendEntryAsync(Server fromServer, int MilisecondsAtWhichReceived)
    // {
    //     await Task.CompletedTask;
    //     return;
    // }
}
