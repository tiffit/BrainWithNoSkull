using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldRay {

    private readonly Vector3D<float> _start;
    private readonly Vector3D<float> _end;
    private readonly int _steps;
    
    public WorldRay(Vector3D<float> pos, Vector3D<float> look, float distance){
        _start = pos;
        _end = _start + Vector3D.Normalize(look) * distance;
        _steps = (int)MathF.Ceiling(distance);
    }

    public WorldRayResult? RunBlockTest(WorldBase world){
        HashSet<Vector3D<int>> testedBlocks = [];
        for (int i = 0; i <= _steps; i++) {
            Vector3D<float> testPos = Vector3D.Lerp(_start, _end, i / (float)_steps);
            Vector3D<int> blockPos = new Vector3D<int>((int)MathF.Floor(testPos.X), (int)MathF.Floor(testPos.Y), (int)MathF.Floor(testPos.Z));
            Direction? bestDirection = null;
            float bestDistance = 0;
            Vector3D<int> bestBlockPos = Vector3D<int>.Zero;

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    for (int z = -1; z <= 1; z++) {
                        CheckBlock(world, blockPos + new Vector3D<int>(x, y, z), testedBlocks, ref bestDistance, ref bestDirection, ref bestBlockPos);
                    }
                }
            }

            if (bestDirection != null) {
                return new WorldRayResult{
                    Direction = bestDirection,
                    Block = world.GetBlock(bestBlockPos),
                    Pos = bestBlockPos
                };
            }
        }

        return null;
    }


    private void CheckBlock(WorldBase world, Vector3D<int> blockPos, HashSet<Vector3D<int>> testedBlocks, ref float bestDistance, ref Direction? bestDirection, ref Vector3D<int> bestBlockPos){
        if(!testedBlocks.Add(blockPos))
            return;
        Block block = world.GetBlock(blockPos, out uint config);
        AABB? aabb = block.GetCollisionBox();
        if(aabb == null)
            return;

        if (block is IMultiblock) {
            Vector3D<uint> offset = block.ConfigContainer.GetValue(config, MultiblockBlockConfigType.Instance);
            aabb = new AABB(MathF.Max(aabb.MinX, offset.X), MathF.Max(aabb.MinY, offset.Y), MathF.Max(aabb.MinZ, offset.Z),
                MathF.Min(aabb.MaxX, offset.X + 1), MathF.Min(aabb.MaxY, offset.Y + 1), MathF.Min(aabb.MaxZ, offset.Z + 1));
        }
        
        aabb += world.GetMultiblockOrigin(blockPos);

        List<Tuple<Direction, float>> tests = [];
        
        {// Y Axis
            float start = _start.Y;
            float slope = _end.Y - start;
            float intersectUp = (aabb.MaxY - start) / slope;
            float intersectDown = (aabb.MinY - start) / slope;
            Vector3D<float> intersectPointUp = Vector3D.Lerp(_start, _end, intersectUp);
            Vector3D<float> intersectPointDown = Vector3D.Lerp(_start, _end, intersectDown);

            if (Within(intersectPointUp.X, aabb.MinX, aabb.MaxX) && Within(intersectPointUp.Z, aabb.MinZ, aabb.MaxZ))
                tests.Add(new Tuple<Direction, float>(Direction.Up, intersectUp));
            if (Within(intersectPointDown.X, aabb.MinX, aabb.MaxX) && Within(intersectPointDown.Z, aabb.MinZ, aabb.MaxZ))
                tests.Add(new Tuple<Direction, float>(Direction.Down, intersectDown));
        }
        {// X Axis
            float start = _start.X;
            float slope = _end.X - start;
            float intersectNorth = (aabb.MaxX - start) / slope;
            float intersectSouth = (aabb.MinX - start) / slope;
            Vector3D<float> intersectPointNorth = Vector3D.Lerp(_start, _end, intersectNorth);
            Vector3D<float> intersectPointSouth = Vector3D.Lerp(_start, _end, intersectSouth);

            if (Within(intersectPointNorth.Y, aabb.MinY, aabb.MaxY) && Within(intersectPointNorth.Z, aabb.MinZ, aabb.MaxZ))
                tests.Add(new Tuple<Direction, float>(Direction.North, intersectNorth));
            if (Within(intersectPointSouth.Y, aabb.MinY, aabb.MaxY) && Within(intersectPointSouth.Z, aabb.MinZ, aabb.MaxZ))
                tests.Add(new Tuple<Direction, float>(Direction.South, intersectSouth));
        }
        {// Z Axis
            float start = _start.Z;
            float slope = _end.Z - start;
            float intersectEast = (aabb.MinZ - start) / slope;
            float intersectWest = (aabb.MaxZ - start) / slope;
            Vector3D<float> intersectPointEast = Vector3D.Lerp(_start, _end, intersectEast);
            Vector3D<float> intersectPointWest = Vector3D.Lerp(_start, _end, intersectWest);

            if (Within(intersectPointEast.Y, aabb.MinY, aabb.MaxY) && Within(intersectPointEast.X, aabb.MinX, aabb.MaxX))
                tests.Add(new Tuple<Direction, float>(Direction.East, intersectEast));
            if (Within(intersectPointWest.Y, aabb.MinY, aabb.MaxY) && Within(intersectPointWest.X, aabb.MinX, aabb.MaxX))
                tests.Add(new Tuple<Direction, float>(Direction.West, intersectWest));
        }
        foreach ((Direction dir, float dist) in tests) {
            if(dist < 0)
                continue;
            if (bestDirection == null || dist < bestDistance) {
                bestDirection = dir;
                bestDistance = dist;
                bestBlockPos = blockPos;
            }
        }
    }

    private static bool Within(float val, float min, float max){
        return min - 0.001f <= val && val <= max + 0.001f;
    }
}

public struct WorldRayResult {
    public Vector3D<int> Pos;
    public Block Block;
    public Direction Direction;
}