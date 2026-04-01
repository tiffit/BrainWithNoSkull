using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public unsafe class VkShaderPipeline {

    public readonly VkShaderProgram ShaderProgram;

    public DescriptorSetLayout DescriptorSetLayout;
    public PipelineLayout PipelineLayout;
    public Pipeline GraphicsPipeline;
    public uint SubpassIndex = 0;
    
    public Buffer VertexBuffer;
    public DeviceMemory VertexBufferMemory;
    
    public DescriptorPool DescriptorPool;
    public DescriptorSet[] DescriptorSets = [];
    
    public PipelineLayout ShadowPipelineLayout;
    public Pipeline ShadowPipeline;

    public BufferAllocator Allocator;
    public Dictionary<Mesh, CompiledMeshData?> Meshes = [];
    public Dictionary<BufferAllocator.MemoryBlock, int> MemoryBlockRefCount = [];

    public VertexBuilder VertexBuilder = null!;
    
    public VkShaderPipeline(VkShaderProgram shader, in PhysicalDeviceLimits limits){
        ShaderProgram = shader;
        Allocator = new BufferAllocator(shader.ShType.BufferSize, limits.MinMemoryMapAlignment);
    }
    
    public VertexInputBindingDescription GetBindingDescription(){
        VertexInputBindingDescription desc = new(){
            Binding = 0,
            Stride = ShaderProgram.ShType.VertexSize*4,
            InputRate = VertexInputRate.Vertex
        };
        return desc;
    }

    public VertexInputAttributeDescription[] GetAttributeDescriptions(){
        uint index = 0;
        uint offset = 0;
        return ShaderProgram.ShType.Params.Select(param => {
            Format format = param.Count switch{
                1 => param.AsUint ? Format.R8Uint : Format.R32Sfloat,
                2 => Format.R32G32Sfloat,
                3 => Format.R32G32B32Sfloat,
                _ => Format.Undefined
            };
            VertexInputAttributeDescription desc = new VertexInputAttributeDescription {
                Binding = 0,
                Location = index++,
                Format = format,
                Offset = offset * 4
            };
            offset += (uint)param.Count;
            return desc;
        }).ToArray();
    }

    public void Destroy(Vk vk, Device device){
        vk.DestroyDescriptorPool(device, DescriptorPool, null);
        vk.DestroyDescriptorSetLayout(device, DescriptorSetLayout, null);
        vk.DestroyPipeline(device, GraphicsPipeline, null);
        vk.DestroyPipelineLayout(device, PipelineLayout, null);

        vk.DestroyBuffer(device, VertexBuffer, null);
        vk.FreeMemory(device, VertexBufferMemory, null);
    }
    
}