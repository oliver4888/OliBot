using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OliBot.Classes.Extensions
{
    public static class TimerExtensions
    {
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
