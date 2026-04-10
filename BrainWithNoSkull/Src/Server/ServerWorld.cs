using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Server;

public class ServerWorld : WorldBase {

    private readonly Dictionary<Vector2D<int>, List<ConnectedClientBase>> _listeningClients = new Dictionary<Vector2D<int>, List<ConnectedClientBase>>();
    
    public ServerWorld(WorldSettings settings) : base(settings, true){
    }

    public void AddListener(Vector2D<int> chunkCoords, ConnectedClientBase client){
        if (_listeningClients.TryGetValue(chunkCoords, out List<ConnectedClientBase>? listeningClient)) {
            _listeningClients[chunkCoords] = [client];
            listeningClient.Add(client);
        }else {
            _listeningClients[chunkCoords] = [client];
        }
    }
    
    public IEnumerable<ConnectedClientBase> GetAllListeners(Vector2D<int> chunkCoords){
        return _listeningClients.TryGetValue(chunkCoords, out List<ConnectedClientBase>? listeners) ? listeners : [];
    }

    public override void AddEntity(Entity entity, Vector3D<float> position){
        base.AddEntity(entity, position);
        if (entity.Type != EntityTypeRegistry.Player) {
            Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(position);
            foreach (ConnectedClientBase client in GetAllListeners(chunkCoords)) {
                client.SendPacket(new CbNewEntityPacket(entity));
            }
        }
    }

    public override void SetBlock(Vector3D<int> blockPos, Block block, uint config){
        base.SetBlock(blockPos, block, config);
        
        foreach (ConnectedClientBase client in GetAllListeners(ChunkUtils.ToChunkCoords(blockPos))) {
            client.SendPacket(new CbBlockPacket(blockPos, block, config));
        }
        
        DoBlockUpdate(blockPos, Vector3D<int>.UnitX);
        DoBlockUpdate(blockPos, Vector3D<int>.UnitY);
        DoBlockUpdate(blockPos, Vector3D<int>.UnitZ);
        
        DoBlockUpdate(blockPos, -Vector3D<int>.UnitX);
        DoBlockUpdate(blockPos, -Vector3D<int>.UnitY);
        DoBlockUpdate(blockPos, -Vector3D<int>.UnitZ);
    }

    public void DoBlockUpdate(Vector3D<int> original, Vector3D<int> direction){
        Vector3D<int> toUpdate = original + direction;
        Block block = GetBlock(toUpdate, out uint config);
        if (!block.IsAir()) {
            block.BlockUpdate(this, toUpdate, config, original);
        }
    }
}