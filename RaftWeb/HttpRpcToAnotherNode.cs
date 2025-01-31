using library;
public class HttpRpcToAnotherNode : IServer {
    public int Id { get; }
    public string Url { get; }
    public bool IsPaused { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    int IServer.Id { get => Id; set => throw new NotImplementedException(); }
    public States State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int CurrentTerm { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int ElectionTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int ElectionTimeoutAdjustmentFactor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int NetworkDelay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IServer? RecognizedLeader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public List<RaftLogEntry> LogBook { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public List<IServer> OtherServersList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Dictionary<IServer, int> NextIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int HighestCommittedIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Dictionary<string, string> StateDictionary { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private HttpClient client = new();

    public HttpRpcToAnotherNode(int id, string url)
    {
        Id = id;
        Url = url;
    }

    public void ReceiveAppendEntriesLogFrom(IServer server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null)
    {
        throw new NotImplementedException();
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, RaftLogEntry request)
    {
        throw new NotImplementedException();
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, IEnumerable<RaftLogEntry> request)
    {
        throw new NotImplementedException();
    }

    public void ReceiveAppendEntriesLogResponseFrom(IServer server, AppendEntryResponse response)
    {
        throw new NotImplementedException();
    }

    public void ReceiveClientCommand((string, string) v)
    {
        throw new NotImplementedException();
    }

    public void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
        throw new NotImplementedException();
    }

    public void ReceiveVoteResponseFrom(IServer server, int requestedVoteCurrentTerm, bool voteGiven)
    {
        throw new NotImplementedException();
    }

    public void ResetElectionTimeout()
    {
        throw new NotImplementedException();
    }

    public void PauseTimeSinceHearingFromLeader()
    {
        throw new NotImplementedException();
    }

    public void SendAppendEntriesLogTo(IServer follower)
    {
        //Note: I put this now just to be able to test.
        var request = new RaftLogEntry{
            LogIndex = 0,
            PreviousLogIndex = -1,
            TermNumber = 0,
            PreviousLogTerm = 0
        };

        try
        {
            client.PostAsJsonAsync(Url + "/request/appendEntries", request);
            //await client.PostAsJsonAsync(Url + "/request/appendEntries", request);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public void SendHeartbeatToAllNodes()
    {
        throw new NotImplementedException();
    }

    public void SendRequestForVoteRPCTo(IServer server)
    {
        throw new NotImplementedException();
    }

    public void StartElection()
    {
        throw new NotImplementedException();
    }

    public void WinElection()
    {
        throw new NotImplementedException();
    }

    public void RestartTimeSinceHearingFromLeader()
    {
        throw new NotImplementedException();
    }

    public void IncrementHighestCommittedIndex()
    {
        throw new NotImplementedException();
    }

    public void ApplyEntry(RaftLogEntry logEntry)
    {
        throw new NotImplementedException();
    }

    public void PauseSimulation()
    {
        throw new NotImplementedException();
    }

    public void Resume()
    {
        throw new NotImplementedException();
    }

    public void CommitEntry(int logIndex)
    {
        throw new NotImplementedException();
    }
}