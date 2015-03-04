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
using System.Diagnostics;

namespace heckdarn
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource Exiting;
        public Form1()
        {
            InitializeComponent();
            Exiting = new CancellationTokenSource();
            Application.ThreadExit += HandleThreadExit;
            Thread thread = new Thread(MonitorProcess);
            thread.Start(Exiting.Token);
        }

        private void HandleThreadExit(object sender, EventArgs e)
        {
            Exiting.Cancel(false);
        }

        private void MonitorProcess(object _token)
        {
            CancellationToken token = (CancellationToken)_token;
            for(;;)
            {
                if(token.IsCancellationRequested)
                    return;
                Process[] processes = Process.GetProcessesByName("Unity");
                try
                {
                    if (processes == null || processes.Length == 0)
                    {
                        throw new Exception("Could not find process.");
                    }
                    Process process = processes[0];
                    if (process == null)
                    {
                        throw new Exception("Could not identify process.");
                    }
                }
                catch(Exception ex)
                {
                    this.BeginInvoke((MethodInvoker)(() => { lblProcessStatus.Text = ex.Message; }));
                    Thread.Sleep(100);
                    continue;
                }
                this.BeginInvoke((MethodInvoker)(() => { lblProcessStatus.Text = "Process identified."; }));
                Thread.Sleep(100);
            }
        }
    }
}
