namespace BrainWithNoSkull.Render.Shader;

public class ShaderDetails {

    public static readonly ShaderDetails Shader = new ShaderDetails("shader", 0x100000*4, [VertexParam.Position, VertexParam.Color]);
    public static readonly List<ShaderDetails> AllShaders = [];
    
    public readonly string Name;
    public readonly uint BufferSize;
    public readonly VertexParam[] Params;
    public readonly uint VertexSize;

    public ShaderDetails(string name, uint bufferSize, VertexParam[] vertexParams){
        Name = name;
        Params = vertexParams;
        BufferSize = bufferSize;
        VertexSize = (uint)vertexParams.Sum(param => param.Count);
        AllShaders.Add(this);
    }
    
}