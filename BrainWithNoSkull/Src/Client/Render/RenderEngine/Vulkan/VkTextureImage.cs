using Silk.NET.Vulkan;
using SkiaSharp;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkTextureImage {
    public Image Image;
    public DeviceMemory Memory;
    public ImageView View;
    public Sampler Sampler;
    
    public void CreateImage(SKBitmap bitmap, Vk vk, Device device, PhysicalDevice physicalDevice, QueueFamilies queueFamilies, Queue graphicsQueue, Format format){
        uint width = (uint)bitmap.Width;
        uint height = (uint)bitmap.Height;
        Span<byte> pixels = bitmap.GetPixelSpan();
        ulong imageSize = (ulong)bitmap.ByteCount;
        
        VkBuffers.CreateBuffer(vk, device, physicalDevice, imageSize,
            BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out Buffer stagingBuffer, out DeviceMemory stagingBufferMemory);
        
        void* data;
        vk.MapMemory(device, stagingBufferMemory, 0, imageSize, 0, &data);
        pixels.CopyTo(new Span<byte>(data, (int)imageSize));
        vk.UnmapMemory(device, stagingBufferMemory);
        
        bitmap.Dispose();
        
        VkImages.CreateImage(vk, device, physicalDevice, width, height, format,
            ImageTiling.Optimal, ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit,
            out Image, out Memory);
        
        VkImages.TransitionImageLayout(vk, queueFamilies, graphicsQueue, device, Image,
            format, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        VkImages.CopyBufferToImage(vk, queueFamilies, graphicsQueue, device, stagingBuffer, Image, width, height);
        VkImages.TransitionImageLayout(vk, queueFamilies, graphicsQueue, device, Image,
            format, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
        
        vk.DestroyBuffer(device, stagingBuffer, null);
        vk.FreeMemory(device, stagingBufferMemory, null);
        
        View = VkImages.CreateImageView(vk, device, Image, format);
    }

    public void CreateSampler(bool useAnisotropy, PhysicalDeviceLimits limits, Vk vk, Device device){
        SamplerCreateInfo samplerCreateInfo = new(){
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = useAnisotropy,
            MaxAnisotropy = useAnisotropy ? limits.MaxSamplerAnisotropy : 0,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Linear,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = 0
        };
        if (vk.CreateSampler(device, in samplerCreateInfo, null, out Sampler) != Result.Success) {
            throw new Exception("Unable to create sampler");
        }
    }

    public DescriptorImageInfo ToDescriptorInfo(){
        return new DescriptorImageInfo{
            ImageLayout = ImageLayout.ReadOnlyOptimal,
            ImageView = View,
            Sampler = Sampler
        };
    }

    public void Destroy(Vk vk, Device device){
        vk.DestroySampler(device, Sampler, null);
        vk.DestroyImageView(device, View, null);
        vk.DestroyImage(device, Image, null);
        vk.FreeMemory(device, Memory, null);
    }
}