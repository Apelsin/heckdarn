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

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.DXGI.Resource;
namespace heckdarn
{
    public class HeckDarn
    {
        public class MainUpdateEventArgs : EventArgs { }
        public delegate void MainUpdateHandler(object sender, MainUpdateEventArgs args);
        public event MainUpdateHandler MainUpdate;
        public int MainUpdatePeriod { get; set; }
        public ProcessHelper TargetProcess { get; set; }
        public Screen Screen { get; set; }

        private Thread UpdateThread;
        private CancellationTokenSource Exiting;

        protected int FrameIndex = 0;
        protected double[] FrameTimes = new double[5];
        protected Stopwatch FrameTimer = new Stopwatch();

        private MethodInvoker PostMonitorProcess;

        public double FramesPerSecond { get; private set; }
        public bool IsExiting
        {
            get
            {
                return Exiting.IsCancellationRequested;
            }
        }

        public HeckDarn(IntPtr output_handle)
        {
            Screen = new Screen(output_handle);
        }
        public void Start()
        {
            MainUpdatePeriod = (int)(1000.0 / 60.0);
            UpdateThread = new Thread(MonitorProcess);
            Exiting = new CancellationTokenSource();
            UpdateThread.Start(Exiting.Token);
        }
        public void Stop(MethodInvoker callback = null)
        {
            PostMonitorProcess = callback;
            Exiting.Cancel(false);
        }

        private void MonitorProcess(object _token)
        {
            CancellationToken token = (CancellationToken)_token;
            for (; ; )
            {
                FrameTimer.Restart();
                if (token.IsCancellationRequested)
                {
                    if (PostMonitorProcess != null)
                        PostMonitorProcess();
                    return;
                }
                if (MainUpdate != null)
                    MainUpdate(this, new MainUpdateEventArgs());
                int sleep_time = MainUpdatePeriod - (int)FrameTimer.ElapsedMilliseconds;
                sleep_time = Math.Max(0, sleep_time);
                Thread.Sleep(sleep_time);
                FrameTimes[FrameIndex] = FrameTimer.Elapsed.TotalSeconds;
                FrameIndex = (FrameIndex + 1) % FrameTimes.Length;
                CalculateFramesPerSecond();
            }
        }
        private void CalculateFramesPerSecond()
        {
            double sum = 0.0;
            foreach (double time in FrameTimes)
                sum += time;
            FramesPerSecond = FrameTimes.Length / sum;
        }
    }
}
