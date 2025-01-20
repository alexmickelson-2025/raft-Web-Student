using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class RaftLogEntry
{
    public string Command { get; set; }
    public int TermNumber { get; set; }
    public int LogIndex { get; set; }
}
