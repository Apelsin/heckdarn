using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;

using System.Runtime.InteropServices;

namespace heckdarn
{
    public partial class Form1 : Form
    {
        private delegate void UpdatePreviewDelegate(Image image);
        private HeckDarn Self;
        private Bitmap OverlayImage;
        public Dictionary<string, object> StatusInfo;
        private Stopwatch OverlayUpdateTimer;
        private double OverlayUpdateTime;

        protected int CaptureIndex = 0;
        protected float[] CaptureTimes = new float[5];
        public double CapturesPerSecond { get; private set; }

        public Form1()
        {
            InitializeComponent();
            StatusInfo = new Dictionary<string, object>();
            Self = new HeckDarn(Handle);
            Self.MainUpdate += HandleMainUpdate;

            OverlayUpdateTime = 1.0 / 20.0;
            OverlayUpdateTimer = new Stopwatch();
            OverlayUpdateTimer.Start();

            // Remove all borders from the window
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            // Get the style flags for the window and add transparent & layered flags
            int wl = Window.GetWindowLong(this.Handle, Window.GWL.ExStyle);
            wl = wl | 0x80000 | 0x20; // Transparent | Layered
            // Apply the style flags
            Window.SetWindowLong(Handle, Window.GWL.ExStyle, wl);
            // Make the window visible by setting the alpha (opacity)
            Window.SetLayeredWindowAttributes(Handle, 0, 255, Window.LWA.Alpha);

            Self.Start();
        }
        protected void CreateOverlayImage(Size size)
        {
            pbOverlay.Image = OverlayImage = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
        }
        private int i = 0;
        private int j = 0;
        private void HandleMainUpdate(object sender, HeckDarn.MainUpdateEventArgs args)
        {
            if (Self.TargetProcess == null)
            {
                try
                {
                    Self.TargetProcess = ProcessHelper.FindAllByName("Skype").First();
                    BeginInvoke((MethodInvoker)(() => { lblProcessStatus.Text = "Process identified."; }));
                }
                catch (InvalidOperationException ex)
                {
                    // Don't attempt to call BeginInvoke again
                    throw ex;
                }
                catch (Exception ex)
                {
                    BeginInvoke((MethodInvoker)(() => { lblProcessStatus.Text = ex.Message; }));
                }
            }
            if (Self.TargetProcess != null)
            {
                IntPtr target_handle = Self.TargetProcess.MainWindow.Handle;
                Rectangle target_rect = Self.TargetProcess.MainWindow.GetWindowRect();
                Size target_size = target_rect.Size;
                if (OverlayUpdateTimer.Elapsed.TotalSeconds > OverlayUpdateTime)
                {
                    Invoke((MethodInvoker)(() => { DrawingControl.SuspendDrawing(pbOverlay); }));
                    if (OverlayImage == null || OverlayImage.Size != target_size)
                    {
                        CreateOverlayImage(target_size);
                    }
                    using (Graphics gfx = Graphics.FromImage(OverlayImage))
                    {
                        Self.TargetProcess.MainWindow.Capture(gfx);
                        Self.TargetProcess.MainWindow.FilterOverlay(OverlayImage);
                    }
                    Invoke((MethodInvoker)(() => { DrawingControl.ResumeDrawing(pbOverlay); }));
                    CaptureTimes[CaptureIndex] = (float)OverlayUpdateTimer.Elapsed.TotalSeconds;
                    CaptureIndex = (CaptureIndex + 1) % CaptureTimes.Length;
                    CapturesPerSecond = HeckDarn.CalculateInverseAverage(CaptureTimes);
                    OverlayUpdateTimer.Restart();
                }
                Invoke((MethodInvoker)(() =>
                {
                    Window.SetWindowRect(Handle, target_rect);
                    Window.SetWindowLong(Handle, Window.GWL.Parent, (int)target_handle);
                }));
                
                StatusInfo["UpdateFPS"] = (int)Self.FramesPerSecond;
                StatusInfo["CaptureFPS"] = (int)CapturesPerSecond;

            }
            List<string> status_info_list = new List<string>();
            foreach (var entry in StatusInfo)
                status_info_list.Add(entry.Key + ": " + entry.Value.ToString());
            string status_info_str = String.Join("\r\n", status_info_list);
            BeginInvoke((MethodInvoker)(() => { lblInfo.Text = status_info_str; }));
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (Self.IsExiting)
                base.OnFormClosing(e);
            else
            {
                e.Cancel = true;
                Self.Stop(() => { Invoke((MethodInvoker)Close); });
            }

        }
    }
}
