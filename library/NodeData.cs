namespace library;

public class NodeData() {
    public int Id { get; set; }
    public string? Status { get; set; }
    public int ElectionTimeout { get; set; }
    public int Term { get; set; }
    public IServer? CurrentTermLeader { get; set; }
    public int CommittedEntryIndex { get; set; }
    public List<RaftLogEntry> Log { get; set; }
    public States State { get; set; }
    public int NodeIntervalScalar { get; set; }
    public Dictionary<string, string>? StateDictionary {get;set;}
};