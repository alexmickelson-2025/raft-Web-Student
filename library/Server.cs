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
    public Dictionary<int, bool> AppendEntriesResponseLog = new();

    public void ResetElectionTimeout()
    {
        int val = (Random.Shared.Next() % 150) + 150;
        ElectionTimeout = val;
    }

    public void SendAppendEntriesLogTo(Server follower)
    {
        follower.ReceiveAppendEntriesLogFrom(this, 0); //I need to be able to automatically increment this
    }

    public void ReceiveAppendEntriesLogFrom(Server server, int requestNumber)
    {
        this.RecognizedLeader = server;
        this.SendAppendEntriesResponseTo(server, requestNumber, true);
    }

    private void SendAppendEntriesResponseTo(Server server, int requestNumber, bool v)
    {
        server.ReceiveAppendEntriesLogResponseFrom(this, requestNumber, v);
    }

    public void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool v)
    {

        if (!AppendEntriesResponseLog.ContainsKey(requestNumber))
        {
            AppendEntriesResponseLog.Add(requestNumber, v);
        }
    }

    // public async Task ProcessReceivedAppendEntryAsync(Server fromServer, int MilisecondsAtWhichReceived)
    // {
    //     await Task.CompletedTask;
    //     return;
    // }
}
