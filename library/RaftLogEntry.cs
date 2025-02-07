using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class RaftLogEntry
{
    public (string, string) Command { get; set; } = ("", "");
    public int TermNumber { get; set; }
    public int LogIndex { get; set; }
    public int PreviousLogIndex { get; set; }
    public int PreviousLogTerm { get; set; }

    public int LeaderHighestCommittedIndex { get; set; }

    public IServer? fromServer { get; set; }
    public int FromServerId { get; set; }
}
