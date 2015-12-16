using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet_analyzer
{
    interface PacketAnalyzerFormInterface
    {
        string hostText { get; set; }
        void LogBps(long bps);
        void logText(string text);
        void SetDevicesNames(string[] devices);
    }
}
