using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkImages {
    public static void CreateImage(Vk vk, Device device, PhysicalDevice physicalDevice, uint width, uint height,
        Format format, ImageTiling tiling, ImageUsageFlags usageFlags, MemoryPropertyFlags memoryFlags,
        out Image image, out DeviceMemory memory){
        ImageCreateInfo imageCreateInfo = new(){
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D(width, height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Format = format,
            Tiling = tiling,
            InitialLayout = ImageLayout.Undefined,
            Usage = usageFlags,
            SharingMode = SharingMode.Exclusive,
            Samples = SampleCountFlags.Count1Bit
        };

        if (vk.CreateImage(device, &imageCreateInfo, null, out image) != Result.Success) {
            throw new Exception("Unable to create texture image");
        }

        vk.GetImageMemoryRequirements(device, image, out MemoryRequirements memReqs);

        MemoryAllocateInfo allocInfo = new(){
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memReqs.Size,
            MemoryTypeIndex = VkBuffers.FindBufferMemoryType(vk, physicalDevice, memReqs.MemoryTypeBits, memoryFlags)
        };
        if (vk.AllocateMemory(device, &allocInfo, null, out memory) != Result.Success) {
            throw new Exception("Unable to allocate texture memory");
        }

        vk.BindImageMemory(device, image, memory, 0);
    }

    public static void CopyBufferToImage(Vk vk, QueueFamilies families, Queue graphicsQueue, Device device, Buffer buffer, Image image, uint width, uint height){
        CommandBuffer cmdBuffer = VkBuffers.BeginTransientCommandBuffer(vk, families, device, out CommandPool pool);

        BufferImageCopy imageCopy = new(){
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers{
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D(width, height, 1)
        };
        vk.CmdCopyBufferToImage(cmdBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, &imageCopy);
        
        VkBuffers.EndTransientCommandBuffer(vk, graphicsQueue, device, cmdBuffer, pool);
    }

    public static void TransitionImageLayout(Vk vk, QueueFamilies families, Queue graphicsQueue, Device device,
        Image image, Format format, ImageLayout old, ImageLayout newLayout){
        CommandBuffer cmdBuffer = VkBuffers.BeginTransientCommandBuffer(vk, families, device, out CommandPool pool);
        
        ImageMemoryBarrier barrier = new(){
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = old,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange{
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        PipelineStageFlags srcStage = 0;
        PipelineStageFlags dstStage = 0;

        if (old == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal) {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            srcStage = PipelineStageFlags.TopOfPipeBit;
            dstStage = PipelineStageFlags.TransferBit;
        }else if (old == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal) {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            srcStage = PipelineStageFlags.TransferBit;
            dstStage = PipelineStageFlags.FragmentShaderBit;
        }
        else {
            throw new ArgumentException("Invalid transition image layouts");
        }
        
        vk.CmdPipelineBarrier(cmdBuffer, srcStage, dstStage, 0, 0, null, 0, null, 1, &barrier);
        
        VkBuffers.EndTransientCommandBuffer(vk, graphicsQueue, device, cmdBuffer, pool);
    }
    
    public static ImageView CreateImageView(Vk vk, Device device, Image image, Format format, ImageAspectFlags aspectFlags = ImageAspectFlags.ColorBit){
        ImageViewCreateInfo createInfo = new(){
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = format,
            Components = new ComponentMapping(ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity),
            SubresourceRange = new ImageSubresourceRange{
                AspectMask = aspectFlags,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };
        if (vk.CreateImageView(device, in createInfo, null, out ImageView view) != Result.Success) {
            throw new Exception("Unable to create image view");
        }

        return view;
    }
}