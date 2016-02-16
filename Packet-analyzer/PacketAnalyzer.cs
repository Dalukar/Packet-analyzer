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
        private readonly Form1 form;
        Thread captureThread;
        Thread bpsThread;
        Thread autoStopThread;
        CaptureDeviceList devices;
        public String[] devNames;
        List<TcpConnectionDump> sessions = new List<TcpConnectionDump>();
        System.Net.IPAddress remoteIP;

        Stopwatch SessionWatch = new Stopwatch();
        double delay = 0;
        double bytesIn = 0;
        double bytesOut = 0;
        public int calculateIntervals = 10;
        public int proxyDelay = 0;
        public double initDelay = 0;

        public double uplinkV1;
        public double downlinkV1;
        public double avgDelayV1;
        public double MOSV1;
        public int V1TimeIteration = 1;
        public int V1PacketsIteration = 1;

        public double uplinkV2;
        public double downlinkV2;
        public double avgDelayV2;
        public double MOSV2;
        double[] uplinkV2Array;
        double[] downlinkV2Array;
        double[] delayV2Array;
        int V2ArrayPosition = 0;
        


        double lastPacketTime = 0;
        public string logBuffer;
        public PacketAnalyzer()
        {
            this.form = Program.form;
            string ver = SharpPcap.Version.VersionString;
            logText("SharpPcap " + ver);
            logText("");
            devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                logText("No device found on this machine");
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

            uplinkV1 = 0;
            downlinkV1 = 0;
            avgDelayV1 = 0;
            V1TimeIteration = 1;
            V1PacketsIteration = 1;

            uplinkV2 = 0;
            downlinkV2 = 0;
            avgDelayV2 = 0;
            uplinkV2Array = new double[calculateIntervals];
            downlinkV2Array = new double[calculateIntervals]; ;
            delayV2Array = new double[calculateIntervals]; 


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

            logText
                ("-- The following tcpdump filter will be applied: \"" + filter + "\"");
            logText
                ("-- Listening on " + device.Description);

            if (captureThread != null){captureThread.Abort();}
            if (bpsThread != null){bpsThread.Abort();}
            if (autoStopThread != null) { autoStopThread.Abort(); }
            captureThread = new Thread(() => device.Capture());
            bpsThread = new Thread(() => 
                { 
                    while (true) 
                    {
                        downlinkV1 = (downlinkV1 * V1TimeIteration + bytesIn) / (V1TimeIteration + 1);
                        uplinkV1 = (uplinkV1 * V1TimeIteration + bytesOut) / (V1TimeIteration + 1);
                        V1TimeIteration += 1;

                        uplinkV2Array[V2ArrayPosition] = bytesOut;
                        downlinkV2Array[V2ArrayPosition] = bytesIn;

                        V2ArrayPosition += 1;
                        V2ArrayPosition = V2ArrayPosition % calculateIntervals;
                        uplinkV2 = 0;
                        downlinkV2 = 0;
                        avgDelayV2 = 0;

                        for (int i = 0; i<calculateIntervals; i++)
                        {
                            uplinkV2 += uplinkV2Array[i];
                            downlinkV2 += downlinkV2Array[i];
                            avgDelayV2 += delayV2Array[i];
                        }

                        uplinkV2 = uplinkV2 / calculateIntervals;
                        downlinkV2 = downlinkV2 / calculateIntervals;
                        avgDelayV2 = avgDelayV2 / calculateIntervals;
                       
                        MOSV1 = CalculateMOS(downlinkV1 / 1024, (initDelay + proxyDelay) / 1000);
                        MOSV2 = CalculateMOS(downlinkV2 / 1024, (initDelay + proxyDelay)/ 1000);
                        bytesIn = 0;
                        bytesOut = 0;
                        Thread.Sleep(1000); 
                    }
                });
            autoStopThread  = new Thread(() =>
            {
                while (true)
                {
                    if (SessionWatch.ElapsedMilliseconds - lastPacketTime >=10000)
                    {
                        logText("No packets recieved in 10 sec, closing capture");
                        StopCapture();
                    }
                    Thread.Sleep(3000);
                }
            });
            autoStopThread.Start();
            captureThread.Start();
        }


        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if(!SessionWatch.IsRunning)
            {
                SessionWatch.Start();
            }
            if (!bpsThread.IsAlive)
            {
                bpsThread.Start();
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
                    bytesOut += len;

                }
                else
                {
                    //превый пришедший пакет после установления соединения
                    if (initDelay == 0 && tcpPacket.Syn == false && tcpPacket.Ack == true)
                    {
                        initDelay = SessionWatch.ElapsedMilliseconds;
                    }
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
                        if (dstIp.ToString() != remoteIP.ToString())
                        {
                            avgDelayV1 = (avgDelayV1 * V1PacketsIteration + delay) / (V1PacketsIteration + 1);
                            V1PacketsIteration += 1;

                            delayV2Array[V2ArrayPosition] = delay;
                            V2ArrayPosition += 1;
                            V2ArrayPosition = V2ArrayPosition % calculateIntervals;
                        }
                        rel = dump.AddPacket(tcpPacket, srcIp.ToString());
                        isSessionExist = true;
                        break;
                    }

                }
                if (!isSessionExist)
                {
                    sessions.Add(new TcpConnectionDump(srcIp.ToString(), srcPort, dstIp.ToString(), dstPort, tcpPacket.SequenceNumber, tcpPacket.AcknowledgmentNumber));
                }
                
                logText(SessionWatch.ElapsedMilliseconds + "\tLen: " + len + "\t" + srcIp + ":" + srcPort + "\t->\t" +
                    dstIp + ":" + dstPort + "\t seq: " + tcpPacket.SequenceNumber +"\t(" + rel[0] + ")\t ack: " + tcpPacket.AcknowledgmentNumber + "\t(" +rel[1] +
                    ")\tSYN: " + tcpPacket.Syn + "\tACK: " + tcpPacket.Ack + "\t delay:" + delay/1000);
                //" \n" packet.PrintHex()
            }
        }

        public void StopCapture()
        {
            //double brIn = (double)bytesIn / lastPacketTime * 1000;
            //double brOut = (double)bytesOut / lastPacketTime * 1000;
            logText("-- capture stopped");
            //logText("Average speed (byte/second):\tin: " + brIn + "\tout: " + brOut);
            //logText("Initial delay (msec):\t" + initDelay);
            //logText("MOS:\t" + CalculateMOS(brIn / 1024, initDelay / 1000));
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

        void logText(string text)
        {
            logBuffer += text + "\n";
        }
        public string GetLog()
        {
            string ret = logBuffer;
            logBuffer = "";
            return ret;
        }
    }
}
