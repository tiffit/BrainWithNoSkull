using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks;

public interface IBakedBlockModel {
    IEnumerable<Vertex> GetVertices(Vector3D<float> posOffset, Block block, uint packedConfig, List<Direction> visible);
    IEnumerable<Vertex> GetVerticesForDirection(Direction? direction, Block block, uint packedConfig);
    
    public void GetAllVertices(List<Vertex> vertices, Dictionary<Direction, List<Vertex>> directional, Block block, uint packedConfig){
        vertices.AddRange(GetVerticesForDirection(null, block, packedConfig));
        foreach (Direction direction in Direction.All) {
            List<Vertex> list = new List<Vertex>(GetVerticesForDirection(direction, block, packedConfig));
            if (list.Count > 0) {
                directional[direction] = list;
            }
        }
    }
}

public class SimpleBakedBlockModel: IBakedBlockModel {

    private readonly List<Vertex> _vertices;
    private readonly Dictionary<Direction, List<Vertex>> _directionalVertices;
    
    public SimpleBakedBlockModel(List<Vertex> vertices, Dictionary<Direction, List<Vertex>> directionalVertices){
        _vertices = vertices;
        _directionalVertices = directionalVertices;
    }

    public IEnumerable<Vertex> GetVertices(Vector3D<float> posOffset, Block block, uint packedConfig, List<Direction> visible){
        foreach (Vertex v in _vertices) {
            yield return v with{
                Pos = v.Pos + posOffset
            };
        }
        
        foreach (Direction dir in visible) {
            if (_directionalVertices.TryGetValue(dir, out List<Vertex>? list)) {
                foreach (Vertex v in list) {
                    yield return v with{
                        Pos = v.Pos + posOffset
                    };
                }
            }
        }
    }

    public IEnumerable<Vertex> GetVerticesForDirection(Direction? direction, Block block, uint packedConfig){
        if (direction == null)
            return _vertices;
        return _directionalVertices.TryGetValue(direction, out List<Vertex>? value) ? value : [];
    }
    
}