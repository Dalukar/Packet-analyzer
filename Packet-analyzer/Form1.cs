using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Packet_analyzer
{
    public partial class Form1 : Form, PacketAnalyzerFormInterface
    {
        public string logBuffer;
        Thread logUpdateThread;
        public string hostText
        {
            get { return textHost.Text; }
            set { textHost.Text = value; }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Analyzer.StopCapture();
            if (logUpdateThread != null)
            {
                logUpdateThread.Abort();
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Program.Analyzer.StartCapture(devicesList.SelectedIndex);
            if (logUpdateThread != null) { logUpdateThread.Abort(); }
            logUpdateThread = new Thread(() =>
            {
                while (true)
                {
                    if(logBuffer != "")
                    {
                        Action chTxt = new Action(() =>
                        {
                            logBox.AppendText(logBuffer);
                            logBox.ScrollToCaret();
                            logBuffer = "";
                        });
                        if (InvokeRequired)
                            this.BeginInvoke(chTxt);
                        else chTxt();
                    }
                    Thread.Sleep(1000);
                }
            });
            logUpdateThread.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Program.Analyzer.StopCapture();
        }

        public void SetDevicesNames(string[] devices)
        {
            devicesList.DataSource = devices;
        }

        public void logText(string text)
        {
            logBuffer += text +"\n";
        }

        public void LogBps(long bpsIn, long bpsOut)
        {
            Action chTxt = new Action(() =>
            {
                textBpsOut.Text = Convert.ToString(bpsOut);
                textBpsIn.Text = Convert.ToString(bpsIn);
            });
            if (InvokeRequired)
                this.BeginInvoke(chTxt);
            else chTxt();
        }
    }
}
