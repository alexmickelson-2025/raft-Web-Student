namespace library;

public class VoteResponse {
    public IServer? ServerResponding { get; set; }
    public int ServerRespondingId {get;set;}
    public bool Accepted {get;set;}
    public int TermNumber { get;set;}
}