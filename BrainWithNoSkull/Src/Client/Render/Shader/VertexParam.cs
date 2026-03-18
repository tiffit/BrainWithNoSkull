namespace BrainWithNoSkull.Client.Render.Shader;

public record VertexParam {
    public static readonly VertexParam Position = new VertexParam{Count = 3};
    public static readonly VertexParam Color = new VertexParam{Count = 3};
    public static readonly VertexParam Uv = new VertexParam{Count = 2};
    
    public required int Count{ get; init; }
}