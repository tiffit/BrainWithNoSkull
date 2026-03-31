using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks;

public class JsonModel {
    public required JsonModelNode[] Nodes;
}

public class JsonModelNode {
    public required string Name;
    public required string Id;
    public required ModelQuad[] Quads;
}

public class ModelQuad {
    public required ModelQuadVertex[] Vertices;
    public required int Texture;
    public required Vector3D<float> Normal;
}

public class ModelQuadVertex {
    public required Vector3D<float> Pos;
    public required Vector2D<float> Uv;
}

public class Vec3DConverter : JsonConverter<Vector3D<float>> {
    public override Vector3D<float> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options){
        reader.Read();
        float x = reader.GetSingle();
        reader.Read();
        float y = reader.GetSingle();
        reader.Read();
        float z = reader.GetSingle();
        reader.Read();
        return new Vector3D<float>(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3D<float> value, JsonSerializerOptions options){
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}

public class Vec2DConverter : JsonConverter<Vector2D<float>> {
    public override Vector2D<float> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options){
        reader.Read();
        float x = reader.GetSingle();
        reader.Read();
        float y = reader.GetSingle();
        reader.Read();
        return new Vector2D<float>(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2D<float> value, JsonSerializerOptions options){
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, IncludeFields = true, Converters = [typeof(Vec3DConverter), typeof(Vec2DConverter)])]
[JsonSerializable(typeof(JsonModel))]
public partial class JsonModelSerializerContext : JsonSerializerContext {}