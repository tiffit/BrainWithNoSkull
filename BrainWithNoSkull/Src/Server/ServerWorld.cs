using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
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
}