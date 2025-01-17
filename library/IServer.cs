namespace library
{
    public interface IServer
    {
        int Id { get; set; }
        public States State { get; set; }
        int CurrentTerm { get; set; }
        int ElectionTimeout { get; set; }
        Server? RecognizedLeader { get; set; }

        void ReceiveAppendEntriesLogFrom(Server server, int requestNumber, int requestCurrentTerm);
        void ReceiveAppendEntriesLogResponseFrom(Server server, int requestNumber, bool accepted);
        void ResetElectionTimeout();
        void SendAppendEntriesLogTo(Server follower);
        void SendHeartbeatToAllNodes();
        void SendRequestForVoteRPCTo(Server server);
        void StartElection();
        void WinElection();
    }
}