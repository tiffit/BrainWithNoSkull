namespace BrainWithNoSkull.Client.Render.Shader;

public class ShaderType {

    public static readonly List<ShaderType> AllShaders = [];
    public static readonly ShaderType Shader = new ShaderType("shader", 0x100000*1024, [VertexParam.Position, VertexParam.Color, VertexParam.Uv, VertexParam.Normal]);
    
    public readonly string Name;
    public readonly uint BufferSize;
    public readonly VertexParam[] Params;
    public readonly uint VertexSize;

    public ShaderType(string name, uint bufferSize, VertexParam[] vertexParams){
        Name = name;
        Params = vertexParams;
        BufferSize = bufferSize;
        VertexSize = (uint)vertexParams.Sum(param => param.Count);
        AllShaders.Add(this);
    }
    
}