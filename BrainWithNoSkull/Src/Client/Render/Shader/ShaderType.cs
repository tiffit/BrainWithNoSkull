using BrainWithNoSkull.Client.Render.Shader.PushConstants;

namespace BrainWithNoSkull.Client.Render.Shader;

public class ShaderType {

    public static readonly List<ShaderType> AllShaders = [];
    
    public static readonly ShaderType Skybox = new ShaderType("skybox", 1024, [VertexParam.Position], 
        ShaderFlag.Skybox, PushConstantType.Skybox);
    
    public static readonly ShaderType Shader = new ShaderType("shader", 0x100000*1024, [VertexParam.Position, VertexParam.Color, VertexParam.Uv, VertexParam.Normal, VertexParam.TextureIndex], 
        ShaderFlag.Mesh | ShaderFlag.CastShadow, PushConstantType.Transform);
    
    public static readonly ShaderType Lines = new ShaderType("lines", 1024, [VertexParam.Position, VertexParam.Color], 
        ShaderFlag.Lines, PushConstantType.None);
    
    public static readonly ShaderType WorldPosColor = new ShaderType("worldPosColor", 1024, [VertexParam.Position, VertexParam.Color], 
        ShaderFlag.None, PushConstantType.None);
    
    public static readonly ShaderType GuiText = new ShaderType("font", 1024*1000, [VertexParam.Position, VertexParam.Color, VertexParam.Uv], 
        ShaderFlag.Gui | ShaderFlag.Font, PushConstantType.None);
    
    public static readonly ShaderType Gui = new ShaderType("gui", 1024*1000, [VertexParam.Position, VertexParam.Color, VertexParam.Uv, VertexParam.Normal, VertexParam.TextureIndex], 
        ShaderFlag.Gui, PushConstantType.None);
    
    public static readonly ShaderType GuiColor = new ShaderType("guiColor", 1024*1000, [VertexParam.Position, VertexParam.Color], 
        ShaderFlag.Gui, PushConstantType.None);
    
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