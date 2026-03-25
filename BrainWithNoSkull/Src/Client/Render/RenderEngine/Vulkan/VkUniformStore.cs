using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkUniformStore {
    public Buffer[] Buffers = [];
    public DeviceMemory[] Memories = [];
    public void*[] MappedMemories = [];
    public ulong Size;

    public void Create(Vk vk, Device device, PhysicalDevice physicalDevice, uint imgCount, ulong size){
        Size = size;
        Buffers = new Buffer[imgCount];
        Memories = new DeviceMemory[imgCount];
        MappedMemories = new void*[imgCount];
        for (int i = 0; i < imgCount; i++) {
            VkBuffers.CreateBuffer(vk, device, physicalDevice, Size,
                BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out Buffers[i], out Memories[i]);
            fixed (void** mappedMemory = MappedMemories) {
                vk.MapMemory(device, Memories[i], 0, Size, 0, &mappedMemory[i]);
            }
        }
    }

    public DescriptorBufferInfo CreateBufferInfo(int frame){
        return new DescriptorBufferInfo{
            Buffer = Buffers[frame],
            Offset = 0,
            Range = Vk.WholeSize
        };
    }
}