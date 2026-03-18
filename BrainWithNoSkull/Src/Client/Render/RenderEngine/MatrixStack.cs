using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public class MatrixStack {
    private readonly LinkedList<Matrix4X4<float>> _stack = [];

    public MatrixStack(){
        _stack.AddLast(Matrix4X4<float>.Identity);
    }

    public void PushMatrix(){
        Matrix4X4<float> last = _stack.Last();
        _stack.AddLast(last.Clone());
    }
    
    public void PopMatrix(){
        _stack.RemoveLast();
    }

    public Vector3D<float> Transform(Vector3D<float> vec){
        return Vector3D.Transform(vec, _stack.Last());
    }

    public void Translate(float x, float y, float z){
        _stack.Last!.Value *= Matrix4X4.CreateTranslation(x, y, z);
    }
}