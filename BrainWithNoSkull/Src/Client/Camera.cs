using BrainWithNoSkull.Client.Render;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client;

public class Camera {
    public Vector3D<float> Position;
    public Vector3D<float> Target;
    public float FoV;
    public float Near = 0.1f;
    public float Far = 1000f;
    
    //set by render engine
    public float AspectRatio = 1;

    public Frustum? Frustum;
}