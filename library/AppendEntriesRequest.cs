using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class AppendEntriesRequest
{
    int RequestId { get; set; }
    public Server? RequestFrom { get; set; }
}
