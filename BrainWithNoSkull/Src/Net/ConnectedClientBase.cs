using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public abstract class ConnectedClientBase {

    public ServerPlayerEntity? Entity;
    private readonly HashSet<Vector2D<int>> _loadedChunks = [];
    private int ViewDistance = 8;
    private Vector2D<int>? _lastChunk = null;
    private bool _playerInfoSent = false;

    public ContainerSync? Container;
    private uint _nextContainerId = 1;
    
    public abstract void SendPacket(IPacket packet);

    public virtual void Tick(double delta){
        if (Entity is{ World: not null, Chunk: not null }) {
            if (_lastChunk != Entity.Chunk.Coords) {
                _lastChunk = Entity.Chunk.Coords;
                for (int x = -ViewDistance; x <= ViewDistance; x++) {
                    for (int z = -ViewDistance; z <= ViewDistance; z++) {
                        Vector2D<int> chunkCoords = _lastChunk.Value + new Vector2D<int>(x, z);
                        Chunk? toSend = Entity.World.GetChunk(chunkCoords);
                        if (toSend != null) {
                            chunkCoords = toSend.Coords;
                            if (_loadedChunks.Add(chunkCoords)) {
                                (Entity.World as ServerWorld)!.AddListener(chunkCoords, this);
                                SendPacket(toSend.ToChunkPacket());
                                toSend.SendAllBlockEntities(this);
                            }
                        }
                    }
                }
            }

            if (!_playerInfoSent) {
                _playerInfoSent = true;
                SendPacket(new CbPlayerInfoPacket(Entity.Position, Entity.Id));
            }

            if (Entity.Inventory.DirtySlots.Count > 0) {
                SendPacket(new CbPlayerInventoryPacket(Entity.Inventory));
                Entity.Inventory.DirtySlots.Clear();
            }

            if (Container != null) {
                byte[] dirtyVariables = Container.GetDirtySyncVariables();
                if (dirtyVariables.Length > 1) {
                    SendPacket(new CbUpdateContainerPacket(Container.Id, dirtyVariables));
                }
            }
        }
    }
    
    public void OpenContainer(ContainerSyncType type, ContainerSync sync){
        if(Container != null)
            CloseContainer();
        Container = sync;
        SendPacket(new CbOpenContainerPacket(Container.Id, sync.Container.GetInventorySize(), type));
    }

    public void CloseContainer(){
        Container = null;
    }

    public uint GetNextContainerId(){
        return _nextContainerId++;
    }
}