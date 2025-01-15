using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library;

public class TimeManager
{
    private Stopwatch stopwatch = new();

    public TimeManager()
    {
         stopwatch.Start();
    }

    public long GetCurrentTime()
    {
        return stopwatch.ElapsedMilliseconds;
    }

    public void RestartTime()
    {
        stopwatch.Restart();
    }

}
