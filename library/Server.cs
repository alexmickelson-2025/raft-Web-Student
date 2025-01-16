using System.Diagnostics;
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
    public int CurrentTerm { get; set; }
    public List<Server> Votes = new(); //If it has a Server in it, that means the server has voted for it with that current term.

    public Stopwatch timeSinceHearingFromLeader = new();

    public void ResetElectionTimeout()
    {
        int val = (Random.Shared.Next() % 150) + 150;
        ElectionTimeout = val;
    }

    public void SendAppendEntriesLogTo(Server follower)
    {
        follower.ReceiveAppendEntriesLogFrom(this, 0, this.CurrentTerm); //I need to be able to automatically increment this
    }

    public void ReceiveAppendEntriesLogFrom(Server server, int requestNumber, int requestCurrentTerm)
    {
        this.RecognizedLeader = server;
        if (requestCurrentTerm < this.CurrentTerm)
        {
            this.SendAppendEntriesResponseTo(server, requestNumber, false);
        }
        else
        {
            this.SendAppendEntriesResponseTo(server, requestNumber, true);
            this.State = States.Follower;
            this.timeSinceHearingFromLeader.Reset();
            this.timeSinceHearingFromLeader.Start();
        }
    }

    private void SendAppendEntriesResponseTo(Server server, int requestNumber, bool accepted)
    {
        server.ReceiveAppendEntriesLogResponseFrom(this, requestNumber, accepted);
    }

    public void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool accepted)
    {

        if (!AppendEntriesResponseLog.ContainsKey(requestNumber))
        {
            AppendEntriesResponseLog.Add(requestNumber, accepted);
        }
    }

    public void StartElection()
    {
        this.Votes.Add(this);
    }

    // public async Task ProcessReceivedAppendEntryAsync(Server fromServer, int MilisecondsAtWhichReceived)
    // {
    //     await Task.CompletedTask;
    //     return;
    // }
}
