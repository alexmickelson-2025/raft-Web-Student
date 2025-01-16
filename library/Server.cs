﻿using System.Diagnostics;
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
    public Dictionary<int, Server> VotesCast = new(); //<termNumber, ServerWeVotedFor>

    public List<Server> VotesReceived = new(); //If it has a Server in it, that means the server has voted for it with that current term.

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
        this.VotesReceived.Add(this);
    }

    public void SendRequestForVoteRPCTo(Server server)
    {
        server.ReceiveVoteRequestFrom(this, this.CurrentTerm);
    }

    //The reason we pass the requestedVote current term (even though it's a property on the server requesting) is the server requesting might update its term after we receive it, so we can't trust that property and must specify it
    private void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
        if (requestedVoteCurrentTerm > this.CurrentTerm) {
            if(!VotesCast.ContainsKey(requestedVoteCurrentTerm)) {
                VotesCast.Add(requestedVoteCurrentTerm, serverRequesting);
                SendVoteResponseTo(serverRequesting, requestedVoteCurrentTerm, true);
            }
        }
    }

    private void SendVoteResponseTo(Server serverRequesting, int requestedVoteCurrentTerm, bool voteGiven)
    {
        serverRequesting.ReceiveVoteResponseFrom(this, requestedVoteCurrentTerm, voteGiven);
    }

    private void ReceiveVoteResponseFrom(Server server, int requestedVoteCurrentTerm, bool voteGiven)
    {
        if (voteGiven) { //potential bug: don't I need to also make sure that the vote is being 
            if (!VotesReceived.Contains(server)) {
                VotesReceived.Add(server);
            }
        }
    }

    // public async Task ProcessReceivedAppendEntryAsync(Server fromServer, int MilisecondsAtWhichReceived)
    // {
    //     await Task.CompletedTask;
    //     return;
    // }
}
