namespace BrainWithNoSkull.Render.Shader;

public record VertexParam {
    public static readonly VertexParam Position = new VertexParam{Count = 3};
    public static readonly VertexParam Color = new VertexParam{Count = 3};
    
    public required int Count{ get; init; }
}