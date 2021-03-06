﻿using System;
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
    public partial class Form1 : Form
    {
        Thread logUpdateThread;
        Thread statusUpdateThread;
        Form GraphForm;
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
            if(GraphForm != null)
            {
                GraphForm.Close();
            }
            Program.Analyzer.StopCapture();
            if (logUpdateThread != null)
            {
                logUpdateThread.Abort();
            }
            if (statusUpdateThread != null)
            {
                statusUpdateThread.Abort();
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Program.Analyzer.calculateIntervals = Convert.ToInt32(textCalcInterval.Text);
            Program.Analyzer.DLThreshold = Convert.ToInt32(textThreshold.Text);
            Program.Analyzer.proxyDelay = Convert.ToInt32(textProxyDelay.Text);
            Program.Analyzer.StartCapture(devicesList.SelectedIndex);
            if (logUpdateThread != null) { logUpdateThread.Abort(); }
            logUpdateThread = new Thread(() =>
            {
                while (true)
                {
                    string log = Program.Analyzer.GetLog();
                    if(log != "")
                    {
                        Action chTxt = new Action(() =>
                        {
                            logBox.AppendText(log);
                            logBox.ScrollToCaret();
                            log = "";
                        });
                        if (InvokeRequired)
                            this.BeginInvoke(chTxt);
                        else chTxt();
                    }
                    Thread.Sleep(1000);
                }
            });
                if (statusUpdateThread != null)
                {
                    statusUpdateThread.Abort();
                }
                statusUpdateThread = new Thread(new ThreadStart(UpdateStatusBox));

                statusUpdateThread.Start();
            
            logUpdateThread.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Program.Analyzer.StopCapture();
            if (checkBox1.Checked)
            {
                Program.Analyzer.CalculateVideoMOS();
                string status = "sView:" + Program.Analyzer.sView + "\n" +
                        "sQuality: " + Program.Analyzer.sQuality + "\n" +
                        "sInteraction: " + Program.Analyzer.sInteraction + "\n" +
                        "Video MOS: " + Math.Round(Program.Analyzer.VideoMOS, 1) + "\n";
                statusBox.Text = status;
            }
                
                if (statusUpdateThread != null)
                {
                    statusUpdateThread.Abort();
                }
        }

        public void SetDevicesNames(string[] devices)
        {
            devicesList.DataSource = devices;
        }

        void UpdateStatusBox()
        {
            while(true)
            {
                Action chTxt = new Action(() =>
                {
                    if (!checkBox1.Checked)
                    {
                        string status = "Initial delay:" + Program.Analyzer.initDelay + "\n" +
                            // "------V1------ \n" +
                        "Uplink: " + Program.Analyzer.uplinkV1 + "\n" +
                        "Downlink: " + Program.Analyzer.downlinkV1 + "\n" +
                        "Delay: " + (Program.Analyzer.avgDelayV1 + Program.Analyzer.proxyDelay) / 1000 + "\n" +
                        "MOS: " + Math.Round(Program.Analyzer.MOSV1, 1) + "\n"; //+
                        //"------V2------ \n" +
                        //"Uplink: " + Program.Analyzer.uplinkV2 + "\n" +
                        //"Downlink: " + Program.Analyzer.downlinkV2 + "\n" +
                        //"Delay: " +( Program.Analyzer.avgDelayV2 + Program.Analyzer.proxyDelay) / 1000 + "\n" +
                        //"MOS: " + Program.Analyzer.MOSV2 + "\n";
                        statusBox.Text = status;
                    }
                    else
                    {
                        string status = "Downlink:" + Program.Analyzer.videoMOSUplink + "\n"+
                        "Stalling:" + Program.Analyzer.lastBufferingIntervals + "/6\n";
                        statusBox.Text = status;
                    }
                });
                if (InvokeRequired)
                    this.BeginInvoke(chTxt);
                else chTxt();
                Thread.Sleep(10000);
            }
        }

        private void buttonGraph_Click(object sender, EventArgs e)
        {
            if (GraphForm == null || !GraphForm.Visible)
           {
               GraphForm = new Graph();
               GraphForm.Show();
           }
        }

       
    }
}
