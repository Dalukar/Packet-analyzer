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
        List<TcpConnectionDump> sessions = new List<TcpConnectionDump>();
        System.Net.IPAddress remoteIP;

        Stopwatch globalWatch = new Stopwatch();
        Stopwatch stopWatch = new Stopwatch();
        uint prevSeq = 0;
        long delay = 0;
        long bpsIn = 0;
        long bpsOut = 0;

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
            bpsThread = new Thread(() => 
                { 
                    while (true) 
                    { 
                        form.LogBps(bpsIn, bpsOut); 
                        bpsIn = 0;
                        bpsOut = 0;
                        Thread.Sleep(1000); 
                    }
                });
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

                delay = -1;
                if (dstIp.ToString() == remoteIP.ToString())
                {
                    prevSeq = tcpPacket.AcknowledgmentNumber;
                    stopWatch.Restart();
                    bpsOut += len;

                }
                else
                {
                    bpsIn += len;
                }

                if (srcIp.ToString() == remoteIP.ToString() && tcpPacket.SequenceNumber == prevSeq)
                {
                    delay = stopWatch.ElapsedMilliseconds;
                    //stopWatch.Restart();
                }

                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                uint[] rel= new uint[2];
                bool isSessionExist = false;
                foreach(TcpConnectionDump dump in sessions)
                {
                    if (dump.isEqual(srcIp.ToString(), srcPort, dstIp.ToString(), dstPort))
                    {
                        rel = dump.AddPacket(tcpPacket, srcIp.ToString());
                        isSessionExist = true;
                        break;
                    }

                }
                if (!isSessionExist)
                {
                    sessions.Add(new TcpConnectionDump(srcIp.ToString(), srcPort, dstIp.ToString(), dstPort, tcpPacket.SequenceNumber, tcpPacket.AcknowledgmentNumber));
                }
                
                form.logText(globalWatch.ElapsedMilliseconds + "\tLen: " + len + "\t" + srcIp + ":" + srcPort + "\t->\t" +
                    dstIp + ":" + dstPort + "\t seq: " + tcpPacket.SequenceNumber +"\t(" + rel[0] + ")\t ack: " + tcpPacket.AcknowledgmentNumber + "\t(" +rel[1] +
                    ")\tSYN: " + tcpPacket.Syn + "\tACK: " + tcpPacket.Ack + "\t delay:" + delay);
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
