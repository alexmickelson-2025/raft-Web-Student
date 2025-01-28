
namespace library
{
    public interface IServer
    {
        public bool IsPaused { get; set; }
        int Id { get; set; }
        public States State { get; set; }
        int CurrentTerm { get; set; }
        int ElectionTimeout { get; set; }

        public int ElectionTimeoutAdjustmentFactor { get; set; }
        public int NetworkDelay { get; set; }
        IServer? RecognizedLeader { get; set; }
        //I might make it a dictionary later. I do need to be able to check term and index, and make sure these things go in order. Linked list?
        List<RaftLogEntry> LogBook { get; set; }
        public List<IServer> OtherServersList { get; set; }
        public Dictionary<IServer, int> NextIndex { get; set; }
        int HighestCommittedIndex { get; set; }
        public Dictionary<string, string> StateDictionary { get; set; }

        void ReceiveAppendEntriesLogFrom(IServer server, int requestNumber, int requestCurrentTerm, RaftLogEntry? logEntry = null);
        void ReceiveAppendEntriesLogFrom(IServer leader, RaftLogEntry request); //delete this one in a minute
        void ReceiveAppendEntriesLogFrom(IServer leader, IEnumerable<RaftLogEntry> request);
        public void ReceiveAppendEntriesLogResponseFrom(IServer server, AppendEntryResponse response);
        //void ReceiveAppendEntriesLogResponseFrom(IServer server, int requestNumber, bool accepted);
        void ReceiveClientCommand((string, string) v);
        void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm);
        public void ReceiveVoteResponseFrom(IServer server, int requestedVoteCurrentTerm, bool voteGiven);
        void ResetElectionTimeout();
        void PauseTimeSinceHearingFromLeader();
        void SendAppendEntriesLogTo(IServer follower);
        void SendHeartbeatToAllNodes();
        void SendRequestForVoteRPCTo(IServer server);
        void StartElection();
        void WinElection();
        void RestartTimeSinceHearingFromLeader();
        void IncrementHighestCommittedIndex();
        void ApplyEntry(RaftLogEntry logEntry);
        void PauseSimulation();
        void Resume();
        public void CommitEntry(int logIndex);
    }
}