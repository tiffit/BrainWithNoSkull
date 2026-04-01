using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks;

public interface IBakedBlockModel {
    IEnumerable<Vertex> GetVertices(Vector3D<float> posOffset, List<Direction> visible);
}

public class SimpleBakedBlockModel: IBakedBlockModel {

    private readonly List<Vertex> _vertices;
    private readonly Dictionary<Direction, List<Vertex>> _directionalVertices;
    
    public SimpleBakedBlockModel(List<Vertex> vertices, Dictionary<Direction, List<Vertex>> directionalVertices){
        _vertices = vertices;
        _directionalVertices = directionalVertices;
    }

    public IEnumerable<Vertex> GetVertices(Vector3D<float> posOffset, List<Direction> visible){
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
    
}