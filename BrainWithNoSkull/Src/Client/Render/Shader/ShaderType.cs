using BrainWithNoSkull.Client.Render.Shader.PushConstants;

namespace BrainWithNoSkull.Client.Render.Shader;

public class ShaderType {

    public static readonly List<ShaderType> AllShaders = [];
    public static readonly ShaderType Skybox = new ShaderType("skybox", 1024, [VertexParam.Position], ShaderFlag.Skybox, PushConstantType.Skybox);
    public static readonly ShaderType Shader = new ShaderType("shader", 0x100000*1024, [VertexParam.Position, VertexParam.Color, VertexParam.Uv, VertexParam.Normal], 
        ShaderFlag.Mesh | ShaderFlag.CastShadow, PushConstantType.Transform);
    
    public readonly string Name;
    public readonly uint BufferSize;
    public readonly VertexParam[] Params;
    public readonly uint VertexSize;
    public readonly ShaderFlag Flags;

    public readonly PushConstantType PushConstant;

    public ShaderType(string name, uint bufferSize, VertexParam[] vertexParams, ShaderFlag flags, PushConstantType pushConstant){
        Name = name;
        Params = vertexParams;
        BufferSize = bufferSize;
        Flags = flags;
        PushConstant = pushConstant;
        VertexSize = (uint)vertexParams.Sum(param => param.Count);
        AllShaders.Add(this);
    }

    public bool HasFlag(ShaderFlag flag){
        return (Flags & flag) != 0;
    }
    
}