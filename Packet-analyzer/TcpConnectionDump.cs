using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet_analyzer
{
    class TcpConnectionDump
    {
        System.Net.IPAddress ip1;
        System.Net.IPAddress ip2;
        int port1;
        int port2;
        int seq1;
        int seq2;
        List<PacketDotNet.Packet> packetsIn = new List<PacketDotNet.Packet>();
        List<PacketDotNet.Packet> packetsOut = new List<PacketDotNet.Packet>();
    }
}
