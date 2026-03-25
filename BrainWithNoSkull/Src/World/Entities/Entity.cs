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
    
    public virtual void Tick(){}
}