using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Entities;

public class Entity {
    private static uint _nextId;

    public EntityType Type;
    public uint Id = _nextId++;
    protected WorldBase? World;
    public Chunk? Chunk;
    
    private Vector3D<float> _position;
    public bool IsPositionDirty = true;
    
    public Entity(EntityType type, WorldBase? world){
        World = world;
        Type = type;
    }

    public void SetPosition(Vector3D<float> pos){
        if (pos != _position) {
            _position = pos;
            IsPositionDirty = true;
        }
    }
    
    public virtual void Tick(){}
}