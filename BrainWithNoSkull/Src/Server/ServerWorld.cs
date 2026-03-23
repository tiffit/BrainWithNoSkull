using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Server;

public class ServerWorld : WorldBase {
    
    public ServerWorld(WorldSettings settings) : base(settings, false){
    }

    public void AddEntity(Entity entity, Vector3D<float> position){
        Chunk chunk = GetChunk(ChunkUtils.ToChunkCoords(position))!;
        chunk.AddEntity(entity);
        entity.SetPosition(position);
    }
}