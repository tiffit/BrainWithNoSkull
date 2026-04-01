using System.Text.Json;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked;

public class JsonBlockModel : UnbakedBlockModel {
    private readonly JsonModel _model;
    private readonly List<string> _textures;

    public JsonBlockModel(string modelName, params string[] textures){
        using FileStream stream = new FileStream($"./Assets/Models/Block/{modelName}.json", FileMode.Open);
        JsonModel? model = (JsonModel?)JsonSerializer.Deserialize(stream, typeof(JsonModel), JsonModelSerializerContext.Default);

        _model = model ?? throw new Exception("Invalid model " + modelName);
        _textures = new HashSet<string>(textures).ToList();
    }

    public override void GatherDependencies(){
        TextureDependencies = new List<string>(_textures);
    }

    public override SimpleBakedBlockModel Bake(TextureResolver resolver){
        List<Vertex> vertices = [];
        foreach (JsonModelNode node in _model.Nodes) {
            foreach (ModelQuad quad in node.Quads) {
                ModelQuadVertex v1 = quad.Vertices[0];
                ModelQuadVertex v2 = quad.Vertices[1];
                ModelQuadVertex v3 = quad.Vertices[2];
                ModelQuadVertex v4 = quad.Vertices[3];

                vertices.AddRange(CubeBlockModel.CreateQuad(
                    in v1.Pos, in v2.Pos, in v3.Pos, in v4.Pos,
                    in v1.Uv, in v2.Uv, in v3.Uv, in v4.Uv,
                    in quad.Normal, resolver(_textures[quad.Texture])
                ));
            }
        }

        return new SimpleBakedBlockModel(vertices, new Dictionary<Direction, List<Vertex>>());
    }
}