using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;

namespace Packet_analyzer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static PacketAnalyzer Analyzer;
        public static Form1 form;
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Analyzer = new PacketAnalyzer();
            Application.Run(form);
        }
    }
}
