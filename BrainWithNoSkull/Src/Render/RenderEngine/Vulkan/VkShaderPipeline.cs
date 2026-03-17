using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace BrainWithNoSkull.Render.RenderEngine.Vulkan;

public unsafe class VkShaderPipeline {

    public readonly VkShaderProgram ShaderProgram;

    public DescriptorSetLayout DescriptorSetLayout;
    public PipelineLayout PipelineLayout;
    public Pipeline GraphicsPipeline;
    public Buffer StagingVertexBuffer;
    public DeviceMemory StagingVertexBufferMemory;
    public Buffer VertexBuffer;
    public DeviceMemory VertexBufferMemory;

    public Buffer[] UniformBuffers = [];
    public DeviceMemory[] UniformBuffersMemory = [];
    
    public VkShaderPipeline(VkShaderProgram shader){
        ShaderProgram = shader;
    }
    
    public VertexInputBindingDescription GetBindingDescription(){
        VertexInputBindingDescription desc = new(){
            Binding = 0,
            Stride = ShaderProgram.Details.VertexSize*4,
            InputRate = VertexInputRate.Vertex
        };
        return desc;
    }

    public VertexInputAttributeDescription[] GetAttributeDescriptions(){
        uint index = 0;
        uint offset = 0;
        return ShaderProgram.Details.Params.Select(param => {
            Format format = param.Count switch{
                1 => Format.R32Sfloat,
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
        vk.DestroyDescriptorSetLayout(device, DescriptorSetLayout, null);
        vk.DestroyPipeline(device, GraphicsPipeline, null);
        vk.DestroyPipelineLayout(device, PipelineLayout, null);
        vk.DestroyBuffer(device, StagingVertexBuffer, null);
        vk.FreeMemory(device, StagingVertexBufferMemory, null);
        vk.DestroyBuffer(device, VertexBuffer, null);
        vk.FreeMemory(device, VertexBufferMemory, null);
    }
    
}