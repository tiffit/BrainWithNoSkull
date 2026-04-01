using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class CollisionTest {

    private readonly WorldBase _world;
    private readonly AABB _aabb;
    private readonly Vector3D<float> _movement;
    
    public CollisionTest(WorldBase world, AABB aabb, Vector3D<float> movement){
        _world = world;
        _aabb = aabb;
        _movement = movement;
    }

    public Vector3D<float> RunTest(out List<AABB> collidedBlocks){
        Vector3D<float> resultMovement = new Vector3D<float>(_movement.X, _movement.Y, _movement.Z);
        collidedBlocks = [];

        if (resultMovement.X != 0) {
            AABB resultBox = AABB.ContainsBoth(_aabb, _aabb + new Vector3D<float>(_movement.X, 0, 0));
            foreach (Vector3D<int> pos in _world.GetBlocksInBox(resultBox)) {
                AABB? blockBox = _world.GetBlock(pos).GetCollisionBox();
                if (blockBox != null) {
                    blockBox += pos;
                    if (resultBox.Intersects(blockBox)) {
                        if (resultMovement.X < 0) {
                            resultMovement.X = MathF.Min(0, resultMovement.X + (blockBox.MaxX + 0.001f - resultBox.MinX));
                        }else if (resultMovement.X > 0) {
                            resultMovement.X = MathF.Max(0, blockBox.MinX - 0.001f - resultBox.MaxX);
                        }
                        collidedBlocks.Add(blockBox);
                    }
                } 
            }
        }

        if (resultMovement.Y != 0) {
            AABB resultBox = AABB.ContainsBoth(_aabb, _aabb + new Vector3D<float>(0, _movement.Y, 0));
            foreach (Vector3D<int> pos in _world.GetBlocksInBox(resultBox)) {
                AABB? blockBox = _world.GetBlock(pos).GetCollisionBox();
                if (blockBox != null) {
                    blockBox += pos;
                    if (resultBox.Intersects(blockBox)) {
                        if (resultMovement.Y < 0) {
                            resultMovement.Y = MathF.Min(0, resultMovement.Y + (blockBox.MaxY + 0.001f - resultBox.MinY));
                        }
                        else if (resultMovement.Y > 0) {
                            resultMovement.Y = MathF.Max(0, blockBox.MinY - 0.001f - resultBox.MaxY);
                        }
                        collidedBlocks.Add(blockBox);
                    }
                }
            }
        }

        if (resultMovement.Z != 0) {
            AABB resultBox = AABB.ContainsBoth(_aabb, _aabb + new Vector3D<float>(0, 0, _movement.Z));
            foreach (Vector3D<int> pos in _world.GetBlocksInBox(resultBox)) {
                AABB? blockBox = _world.GetBlock(pos).GetCollisionBox();
                if (blockBox != null) {
                    blockBox += pos;
                    if (resultBox.Intersects(blockBox)) {
                        if (resultMovement.Z < 0) {
                            resultMovement.Z = MathF.Min(0, resultMovement.Z + (blockBox.MaxZ + 0.001f - resultBox.MinZ));
                        }
                        else if (resultMovement.Z > 0) {
                            resultMovement.Z = MathF.Max(0, blockBox.MinZ - 0.001f - resultBox.MaxZ);
                        }
                        collidedBlocks.Add(blockBox);
                    }
                }
            }
        }

        if (resultMovement.X != 0 && resultMovement.Z != 0) {
            AABB resultBox = AABB.ContainsBoth(_aabb, _aabb + new Vector3D<float>(resultMovement.X, 0, resultMovement.Z));
            foreach (Vector3D<int> pos in _world.GetBlocksInBox(resultBox)) {
                AABB? blockBox = _world.GetBlock(pos).GetCollisionBox();
                if (blockBox != null) {
                    blockBox += pos;
                    if (resultBox.Intersects(blockBox)) {
                        resultMovement = new Vector3D<float>(0, resultMovement.Y, 0);
                    }
                    collidedBlocks.Add(blockBox);
                }
            }
        }
        
        if (resultMovement.LengthSquared > 0) {
            AABB resultBox = AABB.ContainsBoth(_aabb, _aabb + resultMovement);
            foreach (Vector3D<int> pos in _world.GetBlocksInBox(resultBox)) {
                AABB? blockBox = _world.GetBlock(pos).GetCollisionBox();
                if (blockBox != null) {
                    blockBox += pos;
                    if (resultBox.Intersects(blockBox)) {
                        resultMovement = Vector3D<float>.Zero;
                    }
                    collidedBlocks.Add(blockBox);
                }
            }
        }

        return resultMovement;
    }
}