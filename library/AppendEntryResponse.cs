using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class AppendEntryResponse
{
    public int TermNumber { get; set; }
    public int LogIndex { get; set; }
    public bool Accepted { get; set; } = false;
    public int ServerRespondingId {get;set;}
}
