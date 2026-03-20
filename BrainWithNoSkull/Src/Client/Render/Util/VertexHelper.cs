using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Util;

public class VertexHelper {

    public static void AddQuad(VertexBuilder builder, Vector3D<float> p1, Vector3D<float> p2, Vector3D<float> p3, Vector3D<float> p4,
        Vector2D<float> uv1, Vector2D<float> uv2, Vector2D<float> uv3, Vector2D<float> uv4, Vector3D<float>? color = null, bool transformPos = true){
        color ??= Vector3D<float>.One;
        builder.Pos(p1, transformPos).Color(color.Value).Uv(uv1).Build();
        builder.Pos(p3, transformPos).Color(color.Value).Uv(uv3).Build();
        builder.Pos(p4, transformPos).Color(color.Value).Uv(uv4).Build();
        
        builder.Pos(p1, transformPos).Color(color.Value).Uv(uv1).Build();
        builder.Pos(p2, transformPos).Color(color.Value).Uv(uv2).Build();
        builder.Pos(p3, transformPos).Color(color.Value).Uv(uv3).Build();
    }
    
}