using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked;

public class CubeBlockModel : UnbakedBlockModel {
    private static readonly Vector3D<float> P000 = new Vector3D<float>(0, 0, 0);
    private static readonly Vector3D<float> P100 = new Vector3D<float>(1, 0, 0);
    private static readonly Vector3D<float> P110 = new Vector3D<float>(1, 1, 0);
    private static readonly Vector3D<float> P010 = new Vector3D<float>(0, 1, 0);
    private static readonly Vector3D<float> P001 = new Vector3D<float>(0, 0, 1);
    private static readonly Vector3D<float> P101 = new Vector3D<float>(1, 0, 1);
    private static readonly Vector3D<float> P111 = new Vector3D<float>(1, 1, 1);
    private static readonly Vector3D<float> P011 = new Vector3D<float>(0, 1, 1);
    
    private static readonly Vector2D<float> Uv00 = new Vector2D<float>(0, 0);
    private static readonly Vector2D<float> Uv10 = new Vector2D<float>(1, 0);
    private static readonly Vector2D<float> Uv11 = new Vector2D<float>(1, 1);
    private static readonly Vector2D<float> Uv01 = new Vector2D<float>(0, 1);
    
    private readonly string _top, _bottom, _north, _east, _south, _west;

    public CubeBlockModel(string all) : this(all, all, all, all, all, all){}

    public CubeBlockModel(string top, string bottom, string north, string east, string south, string west){
        _top = top;
        _bottom = bottom;
        _north = north;
        _east = east;
        _south = south;
        _west = west;
    }
    
    public override void GatherDependencies(){
        TextureDependencies = new HashSet<string>([_top, _bottom, _north, _east, _south, _west]).ToList();
    }

    public override SimpleBakedBlockModel Bake(TextureResolver resolver, uint packedConfig){
        
        Dictionary<Direction, List<Vertex>> dict = new Dictionary<Direction, List<Vertex>>{
            [Direction.East ] = CreateQuad(in P001, in P011, in P111, in P101, in Uv01, in Uv00, in Uv10, in Uv11, resolver(_east)),
            [Direction.West ] = CreateQuad(in P000, in P100, in P110, in P010, in Uv01, in Uv11, in Uv10, in Uv00, resolver(_west)),
            [Direction.North] = CreateQuad(in P100, in P101, in P111, in P110, in Uv01, in Uv11, in Uv10, in Uv00, resolver(_north)),
            [Direction.South] = CreateQuad(in P000, in P010, in P011, in P001, in Uv01, in Uv00, in Uv10, in Uv11, resolver(_south)),
            [Direction.Up   ] = CreateQuad(in P010, in P110, in P111, in P011, in Uv00, in Uv01, in Uv11, in Uv10, resolver(_top)),
            [Direction.Down ] = CreateQuad(in P000, in P001, in P101, in P100, in Uv00, in Uv01, in Uv11, in Uv10, resolver(_bottom))
        };

        return new SimpleBakedBlockModel([], dict);
    }
    
    private static List<Vertex> CreateQuad(in Vector3D<float> p1, in Vector3D<float> p2, in Vector3D<float> p3, in Vector3D<float> p4,
        in Vector2D<float> uv1, in Vector2D<float> uv2, in Vector2D<float> uv3, in Vector2D<float> uv4, uint texIndex){
        
        Vector3D<float> normal = Vector3D.Normalize(Vector3D.Cross(p3 - p1, p4 - p1));
        return CreateQuad(in p1, in p2, in p3, in p4, in uv1, in uv2, in uv3, in uv4, normal, texIndex);
    }
    
    public static List<Vertex> CreateQuad(in Vector3D<float> p1, in Vector3D<float> p2, in Vector3D<float> p3, in Vector3D<float> p4,
        in Vector2D<float> uv1, in Vector2D<float> uv2, in Vector2D<float> uv3, in Vector2D<float> uv4, in Vector3D<float> normal, uint texIndex){
        
        List<Vertex> list =[
            new Vertex{ Pos = p1, Color = Vector3D<float>.One, Normal = normal, Uv = uv1, TextureIndex = texIndex },
            new Vertex{ Pos = p3, Color = Vector3D<float>.One, Normal = normal, Uv = uv3, TextureIndex = texIndex },
            new Vertex{ Pos = p4, Color = Vector3D<float>.One, Normal = normal, Uv = uv4, TextureIndex = texIndex },

            new Vertex{ Pos = p1, Color = Vector3D<float>.One, Normal = normal, Uv = uv1, TextureIndex = texIndex },
            new Vertex{ Pos = p2, Color = Vector3D<float>.One, Normal = normal, Uv = uv2, TextureIndex = texIndex },
            new Vertex{ Pos = p3, Color = Vector3D<float>.One, Normal = normal, Uv = uv3, TextureIndex = texIndex }
        ];

        return list;
    }
}