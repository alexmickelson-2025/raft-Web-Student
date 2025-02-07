using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class VoteRequest
{
    public IServer? ServerRequestingVote { get; set; }
    public int requestingVoteId { get; set; }
    public int CurrentTerm { get; set; }
}
