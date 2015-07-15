using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Runtime.InteropServices;

namespace heckdarn
{
    public class Window
    {
        public enum GWL
        {
            ExStyle = -20,
            Parent = -8,
        }

        public enum WS_EX
        {
            Transparent = 0x20,
            Layered = 0x80000
        }

        public enum LWA
        {
            ColorKey = 0x1,
            Alpha = 0x2
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public Rectangle Rectangle
            {
                get
                {
                    return new Rectangle(left, top, right - left, bottom - top);
                }
                set
                {
                    left = value.Left;
                    top = value.Top;
                    right = value.Right;
                    bottom = value.Bottom;
                }
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);
        public IntPtr Handle { get; protected set; }
        public Window(IntPtr window_handle)
        {
            Handle = window_handle;
        }
        public Rectangle WindowRect
        {
            get
            {
                RECT rect = new RECT();
                GetWindowRect(Handle, ref rect);
                return rect.Rectangle;
            }
            set
            {
                SetWindowRect(value);
            }
        }
        public static void SetWindowRect(IntPtr handle, Rectangle rectangle, bool repaint = true)
        {
            if (!MoveWindow(handle, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, repaint))
                throw new InvalidOperationException("MoveWindow failed. (todo)");
        }
        public void SetWindowRect(Rectangle rectangle, bool repaint = true)
        {
            SetWindowRect(Handle, rectangle, repaint);
        }
        public static Rectangle GetWindowRect(IntPtr handle)
        {
            RECT rect = new RECT();
            if (GetWindowRect(handle, ref rect))
                return rect.Rectangle;
            else
                throw new InvalidOperationException("GetWindowRect failed. (todo)");
        }
        public Rectangle GetWindowRect()
        {
            return GetWindowRect(Handle);
        }
        public void Capture(Graphics gfx)
        {
            Rectangle rect = GetWindowRect();
            IntPtr hdc = gfx.GetHdc();
            try
            {
                PrintWindow(Handle, hdc, 0);
            }
            finally
            {
                gfx.ReleaseHdc(hdc);
            }
        }
        public unsafe void FilterOverlay(Bitmap bmp)
        {
            Rectangle rect = GetWindowRect();
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Size.Width, bmp.Size.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            
            byte* scan0 = (byte*)bmp_data.Scan0.ToPointer();
            byte bytes_per_pixel = 4;

            for (int i = 0; i < bmp_data.Height; i++)
            {
                byte* row = scan0 + i * bmp_data.Stride;
                for (int j = 0; j < bmp_data.Width; ++j)
                {
                    byte* data = row + j * bytes_per_pixel;
                    byte r = data[0];
                    byte g = data[1];
                    byte b = data[2];
                    data[0] = Filter1(r);
                    data[1] = Filter1(g);
                    data[2] = Filter1(b);
                }
            }
            bmp.UnlockBits(bmp_data);
        }
        private byte Filter1(byte b)
        {
            int c = 255 - b;
            if (c > 127)
                return (byte)((c >> 1) + 64);
            else
                return (byte)(c);
        }
    }
}
