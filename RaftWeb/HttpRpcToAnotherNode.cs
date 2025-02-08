using System.Threading.Tasks;
using library;
public class HttpRpcToAnotherNode : IServer {
    public int Id { get;set; }
    public string Url { get; }
    public bool IsPaused {get;set; }
    public States State {get; set; }
    public int CurrentTerm {get; set; }
    public int ElectionTimeout {get; set; }
    public int ElectionTimeoutAdjustmentFactor {get; set; }
    public int NetworkDelay {get; set; }
    public IServer? RecognizedLeader {get; set; }
    public List<RaftLogEntry> LogBook {get; set; }
    public List<IServer> OtherServersList {get; set; }
    public Dictionary<IServer, int> NextIndex {get; set; }
    public int HighestCommittedIndex {get; set; }
    public Dictionary<string, string> StateDictionary {get; set; }

    private HttpClient client = new();

    public HttpRpcToAnotherNode(int id, string url)
    {
        Id = id;
        Url = url;
    }

    public void ReceiveAppendEntriesLogFrom(IServer server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null)
    {
        Console.WriteLine("ReceiveAppendEntriesLogFrom function with lots of parameters.");
        throw new NotImplementedException();
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, IEnumerable<RaftLogEntry> requests)
    {
        Console.WriteLine("Trying to make post request for a node to receive an append entries log");        

        //for now, will remove later:
        foreach (var req in requests)
        {
            req.FromServerId = leader.Id;
            req.fromServer = leader;
        }
        try {
            client.PostAsJsonAsync(Url + "/request/appendEntries", requests);
        }
        catch (Exception e) {
            Console.WriteLine("Error making post request for receive append entries " + e.Message);
        }
    }

    public void ReceiveAppendEntriesLogResponseFrom(IServer server, AppendEntryResponse response)
    {
        Console.WriteLine("about to make http call to receive append entries log response from Server " + response.ServerRespondingId.ToString());
        try
        {
            client.PostAsJsonAsync(Url + "/response/appendEntries", response);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"node {Url} is down ReceiveAppendEntriesLogResponse. Error was " + e.Message.ToString());
        }
    }

    public void ReceiveClientCommand((string, string) data)
    {
        try
        {
            Console.WriteLine("trying to receive client command now");
            client.PostAsJsonAsync(Url + "/request/command", data);
        }
        catch (Exception e)
        {
            Console.WriteLine("Received error at Receive client command function " + $"{e.Message}");
        }
    }

    public void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
        Console.WriteLine($"received call for ReceiveVoteRequestFrom and about to send http post request. Term {requestedVoteCurrentTerm}.");

        var request = new VoteRequest()
        {
            requestingVoteId = serverRequesting.Id,
            CurrentTerm = requestedVoteCurrentTerm
        };
        try
        {
            client.PostAsJsonAsync(Url + "/request/vote", request);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"node {Url} is down. Tried to send a post request to request a vote from server {serverRequesting.Id} but got exception: ");
            Console.WriteLine(ex.Message.ToString());
        }
    }

    public void ReceiveVoteResponseFrom(IServer server, int requestedVoteCurrentTerm, bool voteGiven)
    {
        Console.WriteLine($"In http rpc to another node we are choosing to try to respond to a vote for term {requestedVoteCurrentTerm}");
        var response = new AppendEntryResponse {
            TermNumber = requestedVoteCurrentTerm,
            LogIndex = 1, //TODO: I don't know what to do for the log index here besides refactor this to receive a voteResponseObject
            Accepted = voteGiven
        };

        try
        {
            Console.WriteLine("Sending post request to cast my vote now");
            client.PostAsJsonAsync(Url + "/response/vote", response);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"node {Url} is down, error trying to cast my vote {e.Message}");
        }
    }

    public void ResetElectionTimeout()
    {
        throw new NotImplementedException();
    }

    public void PauseTimeSinceHearingFromLeader()
    {
        throw new NotImplementedException();
    }

    public void SendAppendEntriesLogRPCTo(IServer follower)
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
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"node {Url} is down, caught error in the sendAppendEntriesLogToServer function " + ex.Message.ToString());
        }
    }

    public void SendHeartbeatRPCToAllNodes()
    {
        Console.WriteLine("Sending heartbeat to all nodes");
        throw new NotImplementedException();
    }

    public void SendRequestForVoteRPCTo(IServer server)
    {
        VoteRequest request = new() {
            ServerRequestingId = this.Id,
            CurrentTerm = this.CurrentTerm
        };

        try {
            client.PostAsJsonAsync(Url+"/request/vote", request);
        }
        catch (Exception e) {
            Console.WriteLine("Error in SendRequestForVoteRPC to function " + e.Message.ToString());
        }
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