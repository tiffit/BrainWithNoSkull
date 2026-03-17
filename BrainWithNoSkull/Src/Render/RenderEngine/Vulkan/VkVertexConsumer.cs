using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrainWithNoSkull.Render.Data;
using BrainWithNoSkull.Render.Shader;
using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Render.RenderEngine.Vulkan;

public class VkVertexConsumer {

    public VertexInputBindingDescription GetBindingDescription(ShaderDetails shaderDetails){
        VertexInputBindingDescription desc = new(){
            Binding = 0,
            Stride = shaderDetails.VertexSize*4,
            InputRate = VertexInputRate.Vertex
        };
        return desc;
    }

    public VertexInputAttributeDescription[] GetAttributeDescriptions(ShaderDetails shaderDetails){
        uint index = 0;
        uint offset = 0;
        return shaderDetails.Params.Select(param => {
            Format format = param.Count switch{
                1 => Format.R32Sfloat,
                2 => Format.R32G32Sfloat,
                3 => Format.R32G32B32Sfloat
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
        // VertexInputAttributeDescription attr1 = new(){
        //     Binding = 0,
        //     Location = 0,
        //     Format = Format.R32G32Sfloat,
        //     Offset = 0
        // };
        // VertexInputAttributeDescription attr2 = new(){
        //     Binding = 0,
        //     Location = 1,
        //     Format = Format.R32G32B32Sfloat,
        //     Offset = 2*4
        // };
        // return [attr1, attr2];
    }
}