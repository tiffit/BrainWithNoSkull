using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkCascade {
    public Framebuffer Framebuffer;
    public ImageView View;
    public float SplitDepth;

    public void Destroy(Vk vk, Device device){
        vk.DestroyImageView(device, View, null);
        vk.DestroyFramebuffer(device, Framebuffer, null);
    }
}