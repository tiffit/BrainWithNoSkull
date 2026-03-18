using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Data;

public class IndVertBuilder {

    private readonly LinkedList<Vertex> _vertices;
    
    private Vector3D<float> _pos;
    private Vector3D<float> _color;
    private Vector2D<float> _uv;

    public IndVertBuilder(LinkedList<Vertex> vertices, float x, float y, float z){
        _pos = new Vector3D<float>(x, y, z);
        _vertices = vertices;
    }

    public IndVertBuilder Color(float r, float g, float b){
        _color = new Vector3D<float>(r, g, b);
        return this;
    }
    
    public IndVertBuilder Color(Vector3D<float> color){
        _color = color;
        return this;
    }
    
    public IndVertBuilder Uv(float u, float v){
        return Uv(new Vector2D<float>(u, v));
    }
    
    public IndVertBuilder Uv(Vector2D<float> uv){
        _uv = uv;
        return this;
    }

    public void Build(){
        Vertex v = new Vertex {
            Pos = _pos,
            Color = _color,
            Uv = _uv
        };
        _vertices.AddLast(v);
    }

}