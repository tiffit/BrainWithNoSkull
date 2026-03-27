using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Shader.PushConstants;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.World;

public class Skybox {

    public static void Render(IRenderEngine renderEngine, LightData lightData){
        VertexBuilder skymapBuilder = renderEngine.GetVertexBuilder(ShaderType.Skybox);
        SkyboxPushConstant skyboxPushConstant = new SkyboxPushConstant{
            LightData = new Vector4D<float>(lightData.LightDir, 0)
        };
        skymapBuilder.PushConstants = MemUtil.StructToByteArray(skyboxPushConstant);
        skymapBuilder.PushConstantStages = ShaderStageFlags.FragmentBit;
        
        const float z = 0f;
        skymapBuilder.Pos(-1, -1, z).Build();
        skymapBuilder.Pos( 1, -1, z).Build();
        skymapBuilder.Pos( 1,  1, z).Build();
        
        skymapBuilder.Pos( 1,  1, z).Build();
        skymapBuilder.Pos(-1,  1, z).Build();
        skymapBuilder.Pos(-1, -1, z).Build();
    }
    
}