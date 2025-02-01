//ctrl + shift + p -> reload window

//TODO: should implement INode, not node
using library;

namespace Simulation;
public class SimulationNode: IServer {
    public int Id { get => ((IServer)InnerNode).Id;  set => ((IServer)InnerNode).Id = value; }
    public readonly Server InnerNode;
    public SimulationNode(Server node)
    {
        this.InnerNode = node;
    }

    public int CurrentTerm { get => ((IServer)InnerNode).CurrentTerm; set => ((IServer)InnerNode).CurrentTerm = value; }
    public int ElectionTimeout { get => ((IServer)InnerNode).ElectionTimeout; set => ((IServer)InnerNode).ElectionTimeout = value; }
    public IServer? RecognizedLeader { get => ((IServer)InnerNode).RecognizedLeader; set => ((IServer)InnerNode).RecognizedLeader = value; }
    public States State { get => ((IServer)InnerNode).State; set => ((IServer)InnerNode).State = value; }
    public int ElectionTimeoutAdjustmentFactor { get => ((IServer)InnerNode).ElectionTimeoutAdjustmentFactor; set => ((IServer)InnerNode).ElectionTimeoutAdjustmentFactor = value; }
    public int NetworkDelay { get; set; }
    public List<RaftLogEntry> LogBook { get => ((IServer)InnerNode).LogBook; set => ((IServer)InnerNode).LogBook = value; }
    public List<IServer> OtherServersList { get => ((IServer)InnerNode).OtherServersList; set => ((IServer)InnerNode).OtherServersList = value; }
    public Dictionary<IServer, int> NextIndex { get => ((IServer)InnerNode).NextIndex; set => ((IServer)InnerNode).NextIndex = value; }
    public int HighestCommittedIndex { get => ((IServer)InnerNode).HighestCommittedIndex; set => ((IServer)InnerNode).HighestCommittedIndex = value; }
    public Dictionary<string, string> StateDictionary { get => ((IServer)InnerNode).StateDictionary; set => ((IServer)InnerNode).StateDictionary = value; }
    public bool IsPaused { get => ((IServer)InnerNode).IsPaused; set => ((IServer)InnerNode).IsPaused = value; }

    public void ReceiveAppendEntriesLogFrom(IServer server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogFrom(server, requestNumber, requestCurrentTerm);
    }

    public void ResetElectionTimeout()
    {
        ((IServer)InnerNode).ResetElectionTimeout();
    }

    public void SendAppendEntriesLogTo(IServer follower)
    {
        ((IServer)InnerNode).SendAppendEntriesLogTo(follower);
    }

    public void SendHeartbeatToAllNodes()
    {
        ((IServer)InnerNode).SendHeartbeatToAllNodes();
    }

    public void SendRequestForVoteRPCTo(Server server)
    {
        ((IServer)InnerNode).SendRequestForVoteRPCTo(server);
    }

    public void StartElection()
    {
        ((IServer)InnerNode).StartElection();
    }

    public void WinElection()
    {
        ((IServer)InnerNode).WinElection();
    }

    public void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm)
    {
        throw new NotImplementedException();
    }

    public void SendRequestForVoteRPCTo(IServer server)
    {
        throw new NotImplementedException();
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, RaftLogEntry request)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogFrom(leader, [request]);
    }

    public void ReceiveClientCommand((string, string) v)
    {
        ((IServer)InnerNode).ReceiveClientCommand(v);
    }

    public void PauseTimeSinceHearingFromLeader()
    {
        ((IServer)InnerNode).PauseTimeSinceHearingFromLeader();
    }

    public void RestartTimeSinceHearingFromLeader()
    {
        ((IServer)InnerNode).RestartTimeSinceHearingFromLeader();
    }

    public void IncrementHighestCommittedIndex()
    {
        ((IServer)InnerNode).IncrementHighestCommittedIndex();
    }

    public void ApplyEntry(RaftLogEntry logEntry)
    {
        ((IServer)InnerNode).ApplyEntry(logEntry);
    }

    public void ReceiveAppendEntriesLogFrom(IServer leader, IEnumerable<RaftLogEntry> request)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogFrom(leader, request);
    }

    public void PauseSimulation()
    {
        ((IServer)InnerNode).PauseSimulation();
    }

    public void Resume()
    {
        ((IServer)InnerNode).Resume();
    }

    public void ReceiveAppendEntriesLogResponseFrom(IServer server, AppendEntryResponse response)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogResponseFrom(server, response);
    }

    public void ReceiveVoteResponseFrom(IServer server, int requestedVoteCurrentTerm, bool voteGiven)
    {
        ((IServer)InnerNode).ReceiveVoteResponseFrom(server, requestedVoteCurrentTerm, voteGiven);
    }

    public void CommitEntry(int logIndex)
    {
        ((IServer)InnerNode).CommitEntry(logIndex);
    }

    //public int Id {get => InnerNode.Id; set => InnerNode.Id = value;}
    //public Server? Leader {get => InnerNode.RecognizedLeader; set => InnerNode.RecognizedLeader = value;}
    ////public int Leader {get => InnerNode.Leader; set => InnerNode.Leader = value;}

    //public States state { get => InnerNode.state; set => InnerNode.state = value; }

    //And I would continue with other node properties

    // public Dictionary<int, bool> AppendEntriesResponseLog = new();
    // public int CurrentTerm { get; set; }

    // public Stopwatch timeSinceHearingFromLeader = new();
}