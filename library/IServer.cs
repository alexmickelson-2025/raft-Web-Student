namespace library
{
    public interface IServer
    {
        int Id { get; set; }
        public States State { get; set; }
        int CurrentTerm { get; set; }
        int ElectionTimeout { get; set; }

        public int ElectionTimeoutAdjustmentFactor { get; set; }
        public int NetworkDelay { get; set; }
        Server? RecognizedLeader { get; set; }

        void ReceiveAppendEntriesLogFrom(Server server, int requestNumber, int requestCurrentTerm);
        void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool accepted);
        void ReceiveVoteRequestFrom(Server serverRequesting, int requestedVoteCurrentTerm);
        void ResetElectionTimeout();
        void SendAppendEntriesLogTo(Server follower);
        void SendHeartbeatToAllNodes();
        void SendRequestForVoteRPCTo(IServer server);
        void StartElection();
        void WinElection();
    }
}