using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldRay {

    private readonly Vector3D<float> _start;
    private readonly Vector3D<float> _end;
    private readonly int _steps;
    
    public WorldRay(Vector3D<float> pos, Vector3D<float> look, float distance){
        _start = pos;
        _end = _start + Vector3D.Normalize(look) * distance;
        _steps = (int)(distance * 2);
    }

    public WorldRayResult? RunBlockTest(WorldBase world){
        for (int i = 0; i <= _steps; i++) {
            Vector3D<float> testPos = Vector3D.Lerp(_start, _end, i / (float)_steps);
            Vector3D<int> blockPos = new Vector3D<int>((int)MathF.Floor(testPos.X), (int)MathF.Floor(testPos.Y), (int)MathF.Floor(testPos.Z));
            Block block = world.GetBlock(blockPos);
            if (!block.IsAir()) {
                return new WorldRayResult{
                    Block = block,
                    Pos = blockPos
                };
            }
        }

        return null;
    }
    
}

public struct WorldRayResult {
    public Vector3D<int> Pos;
    public Block Block;
}