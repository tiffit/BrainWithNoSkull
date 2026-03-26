using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Entities;

public class Entity {
    private AABB _boundingBox = new AABB(-0.4f, 0, -0.4f, 0.4f, 1.5f, 0.4f);
    private static uint _nextId;

    public EntityType Type;
    public uint Id = _nextId++;
    public WorldBase? World;
    public Chunk? Chunk;
    
    public Vector3D<float> Position{ get; private set; }
    
    public bool IsPositionDirty = true;

    public bool OnGround;
    
    public Entity(EntityType type, WorldBase? world){
        World = world;
        Type = type;
    }

    public void SetPosition(Vector3D<float> pos){
        if (pos != Position) {
            Position = pos;
            IsPositionDirty = true;
        }
    }

    public AABB GetBoundingBox(){
        return _boundingBox + Position;
    }

    public virtual void Tick(){
        if (World != null) {
            OnGround = IsOnGround();
        }
    }

    public bool IsOnGround(){
        if (World != null) {
            AABB currentBox = GetBoundingBox();
            AABB groundTestBox = new AABB(currentBox.MinX, currentBox.MinY - 0.1f, currentBox.MinZ, currentBox.MaxX, currentBox.MinY, currentBox.MaxZ);
            return World.GetBlocksInBox(groundTestBox).Any(blockPos => {
                AABB? box = World.GetBlock(blockPos).GetCollisionBox();
                return box != null && (box + blockPos).Intersects(groundTestBox);
            });
        }

        return false;
    }
}