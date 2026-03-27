namespace BrainWithNoSkull.Client.Render.Shader;

public record VertexParam {
    public static readonly VertexParam Position = new VertexParam{Name = "Pos", Count = 3};
    public static readonly VertexParam Color = new VertexParam{Name = "Color", Count = 3};
    public static readonly VertexParam Uv = new VertexParam{Name = "Uv", Count = 2};
    public static readonly VertexParam Normal = new VertexParam{Name = "Norm", Count = 3};
    public static readonly VertexParam TextureIndex = new VertexParam{Name = "texIndex", Count = 1, AsUint = true};
    
    public required int Count{ get; init; }
    public required string Name{ get; init; }
    public bool AsUint{ get; set; }
}