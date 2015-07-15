using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using System.Runtime.InteropServices;

namespace heckdarn
{
    public class Window
    {
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
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        public IntPtr WindowHandle { get; protected set; }
        public Window(IntPtr window_handle)
        {
            WindowHandle = window_handle;
        }
        public Rectangle WindowRect
        {
            get
            {
                RECT rect = new RECT();
                GetWindowRect(WindowHandle, ref rect);
                return rect.Rectangle;
            }
            set
            {
                SetWindowRect(value);
            }
        }
        public void SetWindowRect(Rectangle rectangle, bool repaint=true)
        {
            MoveWindow(WindowHandle, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, repaint);
        }
        public Rectangle GetWindowRect()
        {
            RECT rect = new RECT();
            GetWindowRect(WindowHandle, ref rect);
            return rect.Rectangle;
        }
    }
}
