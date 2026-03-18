using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public struct SwapChainSupportDetails {
    public SurfaceCapabilitiesKHR Capabilities;
    public SurfaceFormatKHR[] Formats;
    public PresentModeKHR[] PresentModes;

    public SurfaceFormatKHR ChooseSwapSurfaceFormat(){
        foreach (SurfaceFormatKHR format in Formats) {
            if (format is{ Format: Format.B8G8R8A8Srgb, ColorSpace: ColorSpaceKHR.SpaceSrgbNonlinearKhr })
                return format;
        }

        return Formats[0];
    }

    public PresentModeKHR ChooseSwapPresentMode(){
        return PresentModes.Contains(PresentModeKHR.MailboxKhr) ? PresentModeKHR.MailboxKhr : PresentModeKHR.FifoKhr;
    }

    public Extent2D ChooseSwapExtent(IWindow window){
        if (Capabilities.CurrentExtent.Width != uint.MaxValue)
            return Capabilities.CurrentExtent;

        Vector2D<int> framebufferSize = window.FramebufferSize;
        Extent2D min = Capabilities.MinImageExtent;
        Extent2D max = Capabilities.MaxImageExtent;
        return new Extent2D{
            Width = Math.Clamp((uint)framebufferSize.X, min.Width, max.Width),
            Height = Math.Clamp((uint)framebufferSize.Y, min.Height, max.Height)
        };
    }

    public uint ChooseImageCount(){
        uint imgCount = Capabilities.MinImageCount + 1;
        if (Capabilities.MaxImageCount > 0 && imgCount > Capabilities.MaxImageCount)
            return Capabilities.MaxImageCount;
        return imgCount;
    }
}