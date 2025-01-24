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
    public Server? RecognizedLeader { get => ((IServer)InnerNode).RecognizedLeader; set => ((IServer)InnerNode).RecognizedLeader = value; }
    public States State { get => ((IServer)InnerNode).State; set => ((IServer)InnerNode).State = value; }
    public int ElectionTimeoutAdjustmentFactor { get => ((IServer)InnerNode).ElectionTimeoutAdjustmentFactor; set => ((IServer)InnerNode).ElectionTimeoutAdjustmentFactor = value; }
    public int NetworkDelay { get; set; }
    public List<RaftLogEntry> LogBook { get => ((IServer)InnerNode).LogBook; set => ((IServer)InnerNode).LogBook = value; }
    public List<IServer> OtherServersList { get => ((IServer)InnerNode).OtherServersList; set => ((IServer)InnerNode).OtherServersList = value; }
    public Dictionary<IServer, int> NextIndex { get => ((IServer)InnerNode).NextIndex; set => ((IServer)InnerNode).NextIndex = value; }
    public int HighestCommittedIndex { get => ((IServer)InnerNode).HighestCommittedIndex; set => ((IServer)InnerNode).HighestCommittedIndex = value; }

    public void ReceiveAppendEntriesLogFrom(Server server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogFrom(server, requestNumber, requestCurrentTerm);
    }

    public void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool accepted)
    {
        ((IServer)InnerNode).ReceiveAppendEntriesLogResponseFrom(server, requestNumber, accepted);
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
        ((IServer)InnerNode).ReceiveAppendEntriesLogFrom(leader, request);
    }

    public void ReceiveClientCommand(string v)
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

    //public int Id {get => InnerNode.Id; set => InnerNode.Id = value;}
    //public Server? Leader {get => InnerNode.RecognizedLeader; set => InnerNode.RecognizedLeader = value;}
    ////public int Leader {get => InnerNode.Leader; set => InnerNode.Leader = value;}

    //public States state { get => InnerNode.state; set => InnerNode.state = value; }

    //And I would continue with other node properties

    // public Dictionary<int, bool> AppendEntriesResponseLog = new();
    // public int CurrentTerm { get; set; }

    // public Stopwatch timeSinceHearingFromLeader = new();
}