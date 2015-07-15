using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.DXGI.Resource;

namespace heckdarn
{
    public class Acquisition : IDisposable
    {
        protected Resource FrameResource;
        public Screen Screen { get; protected set; }
        internal Acquisition(Screen screen)
        {
            Screen = screen;
            Open();
            Capture();
        }
        public void Dispose()
        {
            Post();
            Finish();
            Screen.Acquired = null;
            Screen = null;
        }
        protected void Open()
        {
            if (Screen.Acquired != null)
                throw new InvalidOperationException("Screen already acquired in another context.");
            Screen.Acquired = this;
        }
        public void Capture()
        {
            OutputDuplicateFrameInformation duplicate_frame_info;
            Screen.DuplicatedOutput.AcquireNextFrame(1000, out duplicate_frame_info, out FrameResource);

            // Copy the screen texture to CPU-accessible memory
            using(Texture2D screen_texture = FrameResource.QueryInterfaceOrNull<Texture2D>())
                Screen.Device.ImmediateContext.CopyResource(screen_texture, Screen.Texture);
        }
        public void Post()
        {
            // TODO
        }
        protected void Finish()
        {
            // Free resources
            FrameResource.Dispose();
            Screen.DuplicatedOutput.ReleaseFrame();
        }
    }
}
