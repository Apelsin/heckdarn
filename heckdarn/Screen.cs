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
using MapFlags = SharpDX.DXGI.MapFlags;

namespace heckdarn
{
    public class Screen : IDisposable
    {
        internal Acquisition Acquired;

        internal Texture2DDescription CaptureTextureDescription;
        internal Device Device;
        internal OutputDuplication DuplicatedOutput;
        internal Factory2 Factory;
        internal Output Output;
        internal Output1 Output1;
        public Texture2D Texture { get; private set; }
        public DataBox DataBox { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Acquisition Acquire()
        {
            return new Acquisition(this);
        }

        public Screen(IntPtr output_handle)
        {
            Device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug);
            Factory = new Factory2(true);
            Output = Factory.Adapters1[0].Outputs[0];
            Width = Output.Description.DesktopBounds.Width;
            Height = Output.Description.DesktopBounds.Height;
            CaptureTextureDescription = new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = Width,
                Height = Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
            };
            Output1 = new Output1(Output.NativePointer);
            DuplicatedOutput = Output1.DuplicateOutput(Device);
            Texture = new Texture2D(Device, CaptureTextureDescription);
            DataBox = Device.ImmediateContext.MapSubresource(Texture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
        }
        public void Dispose()
        {
            Device.ImmediateContext.UnmapSubresource(Texture, 0);
            Texture.Dispose();
            DuplicatedOutput.Dispose();
            Factory.Dispose();
            Device.Dispose();
        }
    }
}
