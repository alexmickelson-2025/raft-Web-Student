//ctrl + shift + p -> reload window

//TODO: should implement INode, not node
using library;
namespace Simulation;
public class SimulationNode: Server {
    public readonly SimulationNode InnerNode;
    //public SimulationNode(Server node) {
    //    this.InnerNode = node;
    //}

    public int Id {get => InnerNode.Id; set => InnerNode.Id = value;}
    public Server? Leader {get => InnerNode.RecognizedLeader; set => InnerNode.RecognizedLeader = value;}
    //public int Leader {get => InnerNode.Leader; set => InnerNode.Leader = value;}

    public States state { get => InnerNode.state; set => InnerNode.state = value; }

    //And I would continue with other node properties

    // public Dictionary<int, bool> AppendEntriesResponseLog = new();
    // public int CurrentTerm { get; set; }

    // public Stopwatch timeSinceHearingFromLeader = new();
}