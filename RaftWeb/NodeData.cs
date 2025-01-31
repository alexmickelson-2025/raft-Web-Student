
using library;
namespace NodeData;

public class NodeData(
    int Id {get;set;}
    string Status {get;set;}
    int ElectionTimeout {get;set;}
    int Term {get;set;}
    IServer CurrentTermLeader {get;set;}
    int CommittedEntryIndex {get;set;}
    RaftLogEntry Log {get;set;}
    States State {get;set;}
    int NodeIntervalScalar {get;set;}
  );