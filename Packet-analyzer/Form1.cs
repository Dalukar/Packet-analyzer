using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;
using System.Threading;
using System.Collections;

namespace Packet_analyzer
{
    public partial class Form1 : Form
    {
        public Thread captureThread;
        CaptureDeviceList devices;
        List<PacketDotNet.Packet> packets = new List<PacketDotNet.Packet>();
        public Form1()
        {
            InitializeComponent();

            string ver = SharpPcap.Version.VersionString;
            SafeLog("SharpPcap " + ver);
            SafeLog("");

            devices = CaptureDeviceList.Instance;
            devicesList.DataSource = devices;
            if (devices.Count < 1)
            {
                SafeLog("No device found on this machine");
                return;
            }
 
        }

        private void StartCapture(ICaptureDevice device)
        {
            device.Capture();
        }

        private  void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var time = e.Packet.Timeval.Date;
            var len = e.Packet.Data.Length;

            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var tcpPacket = PacketDotNet.TcpPacket.GetEncapsulated(packet);
            if (tcpPacket != null)
            {
                var ipPacket = (PacketDotNet.IpPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                packets.Add(packet);
                SafeLog(time.Hour + ":" + time.Minute + ":" + time.Second + "," + time.Millisecond + "\tLen=" + len + " " +
                    srcIp + ":" + srcPort + " -> " + dstIp + ":" + dstPort + " \n" + packet.PrintHex());
            }
        }

        private  void SafeLog(string text)
        {
            Action chTxt = new Action(() =>
            {
                logBox.AppendText(text + "\n");
                logBox.ScrollToCaret();
            });
            if (InvokeRequired)
                this.BeginInvoke(chTxt);
            else chTxt();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (captureThread != null)
            {
                captureThread.Abort();
            }
                 
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            var device = devices[devicesList.SelectedIndex];

            device.OnPacketArrival -= 
                new PacketArrivalEventHandler(device_OnPacketArrival); //надеюсь это работает
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(device_OnPacketArrival);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            string filter = "ip and tcp";
            device.Filter = filter;

            SafeLog("");
            SafeLog
                ("-- The following tcpdump filter will be applied: \"" + filter + "\"");
            SafeLog
                ("-- Listening on " + device.Description);

            if (captureThread != null)
            {
                captureThread.Abort();
            }
            captureThread = new Thread(() => StartCapture(device));
            captureThread.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (captureThread != null)
            {
                captureThread.Abort();
            }

        }
    }
}
