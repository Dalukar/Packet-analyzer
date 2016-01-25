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
        Thread autoStopThread;
        CaptureDeviceList devices;
        public String[] devNames;
        List<TcpConnectionDump> sessions = new List<TcpConnectionDump>();
        System.Net.IPAddress remoteIP;

        Stopwatch SessionWatch = new Stopwatch();
        long delay = 0;
        long bpsIn = 0;
        long bpsOut = 0;
        int bytesIn = 0;
        int bytesOut = 0;
        double initDelay = 0;
        double lastPacketTime = 0;
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
            var device = devices[deviceNo];

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
            autoStopThread  = new Thread(() =>
            {
                while (true)
                {
                    if (SessionWatch.ElapsedMilliseconds - lastPacketTime >=3000)
                    {
                        form.logText("No packets recieved in 3 sec, closing capture");
                        StopCapture();
                    }
                    Thread.Sleep(3000);
                }
            });
            autoStopThread.Start();
            bpsThread.Start();
            captureThread.Start();
        }


        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if(!SessionWatch.IsRunning)
            {
                SessionWatch.Start();
            }
            var len = e.Packet.Data.Length;

            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var tcpPacket = (PacketDotNet.TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));
            if (tcpPacket != null)
            {
                var ipPacket = (PacketDotNet.IpPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;

                delay = -1;
                lastPacketTime = SessionWatch.ElapsedMilliseconds;
                if (dstIp.ToString() == remoteIP.ToString())
                {
                    bpsOut += len;
                    bytesOut += len;

                }
                else
                {
                    //превый пришедший пакет после установления соединения
                    if (initDelay == 0 && tcpPacket.Syn == false && tcpPacket.Ack == true)
                    {
                        initDelay = SessionWatch.ElapsedMilliseconds;
                    }
                    bpsIn += len;
                    bytesIn += len;
                }


                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                uint[] rel= new uint[2];
                bool isSessionExist = false;
                foreach(TcpConnectionDump dump in sessions)
                {
                    if (dump.isEqual(srcIp.ToString(), srcPort, dstIp.ToString(), dstPort))
                    {
                        delay = dump.delay.ElapsedMilliseconds;
                        rel = dump.AddPacket(tcpPacket, srcIp.ToString());
                        isSessionExist = true;
                        break;
                    }

                }
                if (!isSessionExist)
                {
                    sessions.Add(new TcpConnectionDump(srcIp.ToString(), srcPort, dstIp.ToString(), dstPort, tcpPacket.SequenceNumber, tcpPacket.AcknowledgmentNumber));
                }
                
                form.logText(SessionWatch.ElapsedMilliseconds + "\tLen: " + len + "\t" + srcIp + ":" + srcPort + "\t->\t" +
                    dstIp + ":" + dstPort + "\t seq: " + tcpPacket.SequenceNumber +"\t(" + rel[0] + ")\t ack: " + tcpPacket.AcknowledgmentNumber + "\t(" +rel[1] +
                    ")\tSYN: " + tcpPacket.Syn + "\tACK: " + tcpPacket.Ack + "\t delay:" + delay);
                //" \n" packet.PrintHex()
            }
        }

        public void StopCapture()
        {
            double brIn = (double)bytesIn / lastPacketTime * 1000;
            double brOut = (double)bytesOut / lastPacketTime * 1000;
            form.logText("-- capture stopped");
            form.logText("Average speed (byte/second):\tin: " + brIn + "\tout: " + brOut);
            form.logText("Initial delay (msec):\t" + initDelay);
            form.logText("MOS:\t" + CalculateMOS(brIn / 1024, initDelay / 1000));
            SessionWatch.Reset();
            initDelay = 0;
            bytesIn = 0;
            bytesOut = 0;
            if (captureThread != null)
            {
                captureThread.Abort();
            }
            if (bpsThread != null)
            {
                bpsThread.Abort();
            }
            if (autoStopThread != null)
            {
                autoStopThread.Abort();
            }
        }

        public double CalculateMOS(double BR, double D)
        {
            //BR - в кб/сек
            // В - в сек
            int a = 1;
            int b = 5;
            double c0 = 2.39;
            double c1 = 0.33;
            double c2 = 1.02;
            double c3 = -0.02;
            double MOS = (b-a)/(1 + c0 * Math.Pow(BR, -c1-c3*D)*Math.Pow(c2,D)) + a;
            return MOS;
        }
    }
}
