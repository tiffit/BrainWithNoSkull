using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkBuffers {
    public static void CreateBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size, BufferUsageFlags usageFlags, MemoryPropertyFlags propFlags, out Buffer buffer, out DeviceMemory memory){
        BufferCreateInfo createInfo = new(){
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usageFlags,
            SharingMode = SharingMode.Exclusive
        };
        if (vk.CreateBuffer(device, in createInfo, null, out buffer) != Result.Success) {
            throw new Exception("Unable to create buffer");
        }
        vk.GetBufferMemoryRequirements(device, buffer, out MemoryRequirements memRequirements);
        MemoryAllocateInfo allocateInfo = new(){
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindBufferMemoryType(vk, physicalDevice, memRequirements.MemoryTypeBits, propFlags)
        };
        if (vk.AllocateMemory(device, &allocateInfo, null, out memory) != Result.Success) {
            throw new Exception("Unable to allocate buffer memory");
        }
        vk.BindBufferMemory(device, buffer, memory, 0);
    }
    
    public static uint FindBufferMemoryType(Vk vk, PhysicalDevice device, uint filter, MemoryPropertyFlags props){
        vk.GetPhysicalDeviceMemoryProperties(device, out PhysicalDeviceMemoryProperties memProperties);

        for (int i = 0; i < memProperties.MemoryTypeCount; i++) {
            if ((filter & (1 << i)) != 0 && (memProperties.MemoryTypes[i].PropertyFlags & props) == props) {
                return (uint)i;
            }
        }

        throw new Exception("Unable to find suitable memory type");
    }

    public static void CopyBuffer(Vk vk, Queue graphicsQueue, QueueFamilies families, Device device, Buffer src, Buffer dst, ulong offset, ulong size){
        CommandBuffer cmdBuffer = BeginTransientCommandBuffer(vk, families, device, out CommandPool pool);
        
        BufferCopy copyCmd = new(0, offset, size);
        vk.CmdCopyBuffer(cmdBuffer, src, dst, 1, &copyCmd);

        EndTransientCommandBuffer(vk, graphicsQueue, device, cmdBuffer, pool);
    }

    public static CommandBuffer BeginTransientCommandBuffer(Vk vk, QueueFamilies families, Device device, out CommandPool pool){
        CommandPoolCreateInfo createInfo = new(){
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.TransientBit,
            QueueFamilyIndex = families.Graphics!.Value
        };
        if (vk.CreateCommandPool(device, in createInfo, null, out pool) != Result.Success) {
            throw new Exception("Unable to create buffer command pool");
        }

        CommandBufferAllocateInfo allocateInfo = new(){
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = pool,
            CommandBufferCount = 1
        };
        vk.AllocateCommandBuffers(device, &allocateInfo, out CommandBuffer cmdBuffer);

        CommandBufferBeginInfo beginInfo = new(){
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        vk.BeginCommandBuffer(cmdBuffer, &beginInfo);
        return cmdBuffer;
    }

    public static void EndTransientCommandBuffer(Vk vk, Queue graphicsQueue, Device device, CommandBuffer cmdBuffer, CommandPool pool){
        vk.EndCommandBuffer(cmdBuffer);

        SubmitInfo submitInfo = new(){
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &cmdBuffer
        };

        vk.QueueSubmit(graphicsQueue, 1, &submitInfo, default);
        vk.QueueWaitIdle(graphicsQueue);

        vk.FreeCommandBuffers(device, pool, 1, &cmdBuffer);
        vk.DestroyCommandPool(device, pool, null);
    }

}