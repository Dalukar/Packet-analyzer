using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet_analyzer
{
    class TcpConnectionDump
    {
        string ip1;
        string ip2;
        int port1;
        int port2;
        uint seq1Start;
        uint seq2Start;
        uint seq1;
        uint seq2;
        List<PacketDotNet.TcpPacket> packetsIn = new List<PacketDotNet.TcpPacket>();
        List<PacketDotNet.TcpPacket> packetsOut = new List<PacketDotNet.TcpPacket>();

        public TcpConnectionDump(string ip1, int port1, string ip2, int port2, uint seq1Start, uint seq2Start)
        {
            this.ip1 = ip1;
            this.ip2 = ip2;
            this.port1 = port1;
            this.port2 = port2;
            this.seq1Start = seq1Start;
            this.seq2Start = seq2Start;
        }

        public uint[] AddPacket(PacketDotNet.TcpPacket packet, string srcIp)
        {
            uint[] rel = new uint[2];
            if (seq2Start == 0)
                seq2Start = packet.SequenceNumber;
            if (ip1 == srcIp)
            {
                packetsOut.Add(packet);
                rel[0] = packet.SequenceNumber - seq1Start;
                rel[1] = packet.AcknowledgmentNumber - seq2Start;
            }
            else
            {
                packetsIn.Add(packet);
                rel[0] = packet.SequenceNumber - seq2Start;
                rel[1] = packet.AcknowledgmentNumber - seq1Start;
            }
            return rel;
        }

        public bool isEqual(string ip1, int port1, string ip2, int port2)
        {
            if ((this.ip1 == ip1 && this.port1 == port1 && this.ip2 == ip2 && this.port2 == port2) ||
                (this.ip1 == ip2 && this.port1 == port2 && this.ip2 == ip1 && this.port2 == port1))
                return true;
            else
                return false;
        }
    }
}
