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
    public partial class Graph : Form
    {
        Thread graphThread;
        public Graph()
        {
            InitializeComponent();

            graphThread = new Thread(() =>
            {
                while (true)
                {
                    chart1.Series[1].Points[0].SetValueY(Program.Analyzer.DLThreshold);
                    chart1.Series[1].Points[1].SetValueY(Program.Analyzer.DLThreshold);
                    int count = Program.Analyzer.DLArray.Count;
                    for (int i = 19; i >= 0; i-- )
                    {                        
                        if(count > i)
                        {
                            chart1.Series[0].Points[19 - i].SetValueY(Program.Analyzer.DLArray[count -1 - i]);
                        }
                    }
                    Action update = new Action(() =>
                    {
                        chart1.ResetAutoValues();
                        chart1.Invalidate();
                    });
                    if (InvokeRequired)
                        this.BeginInvoke(update);
                    else update();
                    
                    Thread.Sleep(1000);
                }
            });
            graphThread.Start();
        }

        private void Graph_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (graphThread != null)
            {
                graphThread.Abort();
            }
        }
    }
}
