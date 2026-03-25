using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Util;

public class VertexHelper {

    public static void AddQuad(VertexBuilder builder, Vector3D<float> p1, Vector3D<float> p2, Vector3D<float> p3, Vector3D<float> p4,
        Vector2D<float> uv1, Vector2D<float> uv2, Vector2D<float> uv3, Vector2D<float> uv4, Vector3D<float>? color = null){
        color ??= Vector3D<float>.One;

        Vector3D<float> normal = Vector3D.Normalize(Vector3D.Cross(p3 - p1, p4 - p1));
        
        builder.Pos(p1).Color(color.Value).Uv(uv1).Normal(normal).Build();
        builder.Pos(p3).Color(color.Value).Uv(uv3).Normal(normal).Build();
        builder.Pos(p4).Color(color.Value).Uv(uv4).Normal(normal).Build();
        
        builder.Pos(p1).Color(color.Value).Uv(uv1).Normal(normal).Build();
        builder.Pos(p2).Color(color.Value).Uv(uv2).Normal(normal).Build();
        builder.Pos(p3).Color(color.Value).Uv(uv3).Normal(normal).Build();
    }
    
}