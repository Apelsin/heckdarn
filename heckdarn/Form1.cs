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



namespace heckdarn
{
    public partial class Form1 : Form
    {
        private delegate void UpdatePreviewDelegate(Image image);
        private HeckDarn Self;
        private Bitmap PreviewImage;
        public Dictionary<string, object> StatusInfo;
        public Form1()
        {
            InitializeComponent();
            StatusInfo = new Dictionary<string, object>();
            Self = new HeckDarn(Handle);
            Self.MainUpdate += HandleMainUpdate;

            int width = Self.Screen.Width;
            int height = Self.Screen.Height;
            PreviewImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            pbWindowPreview.Image = PreviewImage;

            Self.Start();
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
                try
                {
                    using (var acquisition = Self.Screen.Acquire())
                    {
                        int width = Self.Screen.Width;
                        int height = Self.Screen.Height;
                        Rectangle bounds = new Rectangle(0, 0, width, height);

                        // Copy pixels from screen capture Texture to GDI bitmap
                        SharpDX.DataBox source = Self.Screen.DataBox;

                        pbWindowPreview.Invoke((MethodInvoker)(() => { DrawingControl.SuspendDrawing(pbWindowPreview); }));
                        BitmapData destination = PreviewImage.LockBits(bounds, ImageLockMode.WriteOnly, PreviewImage.PixelFormat);

                        IntPtr src_p = source.DataPointer;
                        IntPtr dst_p = destination.Scan0;

                        for (int y = 0; y < height; y++)
                        {
                            // Copy a single line 
                            SharpDX.Utilities.CopyMemory(dst_p, src_p, width * 4);

                            // Advance pointers
                            src_p = IntPtr.Add(src_p, source.RowPitch);
                            dst_p = IntPtr.Add(dst_p, destination.Stride);
                        }

                        PreviewImage.UnlockBits(destination);
                        pbWindowPreview.Invoke((MethodInvoker)(() => { DrawingControl.ResumeDrawing(pbWindowPreview); }));
                        pbWindowPreview.Invoke((MethodInvoker)pbWindowPreview.Invalidate);
                    }
                }
                catch (SharpDX.SharpDXException e)
                {
                    if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        // Probably nothing updated; just try again.
                        //Console.WriteLine("Timed out while trying to acquire result; retrying...");
                        return;
                    }
                    else
                    {
                        throw e;
                    }
                }
                StatusInfo["FPS"] = (int)Self.FramesPerSecond;
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
