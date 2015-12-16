using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace Packet_analyzer
{
    class PacketAnalyzer
    {
        private readonly PacketAnalyzerFormInterface form;
        Thread captureThread;
        Thread bpsThread;
        CaptureDeviceList devices;
        public String[] devNames;
        List<PacketDotNet.Packet> packets = new List<PacketDotNet.Packet>();
        System.Net.IPAddress remoteIP;
        Stopwatch globalWatch = new Stopwatch();
        Stopwatch stopWatch = new Stopwatch();
        uint prevSeq = 0;
        long delay = 0;
        long bps = 0;

        public PacketAnalyzer(PacketAnalyzerFormInterface form)
        {
            this.form = form;
            string ver = SharpPcap.Version.VersionString;
            form.logText("SharpPcap " + ver);
            form.logText("");
            devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                form.logText("No device found on this machine");
                return;
            }
            devNames = new String[devices.Count];
            for (int i = 0; i < devNames.Length; i++)
            {
                devNames[i] = devices[i].Description;
            }
            form.SetDevicesNames(devNames);
        }

        public void StartCapture(int deviceNo)
        {
            var device = devices[deviceNo] as SharpPcap.WinPcap.WinPcapDevice;

            device.OnPacketArrival -=
                new PacketArrivalEventHandler(device_OnPacketArrival); //надеюсь это работает
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(device_OnPacketArrival);

            int readTimeoutMilliseconds = 1;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            //string filter = "ip and tcp";
            string filter = "tcp and host " + form.hostText;
            device.Filter = filter;
            remoteIP = System.Net.IPAddress.Parse(form.hostText);

            form.logText
                ("-- The following tcpdump filter will be applied: \"" + filter + "\"");
            form.logText
                ("-- Listening on " + device.Description);

            if (captureThread != null){captureThread.Abort();}
            if (bpsThread != null){bpsThread.Abort();}
            captureThread = new Thread(() => device.Capture());
            bpsThread = new Thread(() => { while (true) { form.LogBps(bps); bps = 0; Thread.Sleep(1000); } });
            bpsThread.Start();
            stopWatch.Start();
            globalWatch.Start();
            captureThread.Start();
        }


        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var time = e.Packet.Timeval.Date;
            var len = e.Packet.Data.Length;

            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var tcpPacket = (PacketDotNet.TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));
            if (tcpPacket != null)
            {
                var ipPacket = (PacketDotNet.IpPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;

                bps += len;
                delay = -1;
                if (dstIp.ToString() == remoteIP.ToString())
                {
                    prevSeq = tcpPacket.AcknowledgmentNumber;
                    stopWatch.Restart();
                }

                if (srcIp.ToString() == remoteIP.ToString() && tcpPacket.SequenceNumber == prevSeq)
                {
                    delay = stopWatch.ElapsedMilliseconds;
                    //stopWatch.Restart();
                }

                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                packets.Add(packet);
                form.logText(globalWatch.ElapsedMilliseconds + "\tLen: " + len + "\t" + srcIp + ":" + srcPort + "\t->\t" +
                    dstIp + ":" + dstPort + "\t seq: " + tcpPacket.SequenceNumber + "\t ack: " + tcpPacket.AcknowledgmentNumber + "\tSYN: " + tcpPacket.Syn + "\tACK: " + tcpPacket.Ack + "\t delay:" + delay);
                //" \n" packet.PrintHex()
            }
        }

        public void StopCapture()
        {
            if (bpsThread != null)
            {
                bpsThread.Abort();
            }
            if (captureThread != null)
            {
                captureThread.Abort();
            }
            form.logText("-- capture stopped");
        }
    }
}
