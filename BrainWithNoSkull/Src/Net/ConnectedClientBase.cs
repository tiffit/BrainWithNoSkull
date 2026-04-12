using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public abstract class ConnectedClientBase {

    public ServerPlayerEntity? Entity;
    private readonly HashSet<Vector2D<int>> _loadedChunks = [];
    private int ViewDistance = 8;
    private Vector2D<int>? _lastChunk = null;
    private bool _sentAllChunks = false;
    private bool _playerInfoSent = false;

    public ContainerSync? Container;
    private uint _nextContainerId = 1;
    
    public abstract void SendPacket(IPacket packet);

    public virtual void Tick(double delta){
        if (Entity is{ World: not null, Chunk: not null }) {
            if (!_sentAllChunks || _lastChunk != Entity.Chunk.Coords) {
                _lastChunk = Entity.Chunk.Coords;
                _sentAllChunks = true;
                ServerWorld world = (Entity.World as ServerWorld)!;
                for (int x = -ViewDistance; x <= ViewDistance; x++) {
                    for (int z = -ViewDistance; z <= ViewDistance; z++) {
                        Vector2D<int> chunkCoords = _lastChunk.Value + new Vector2D<int>(x, z);
                        Chunk? toSend = world.GetOrQueueChunk(chunkCoords, Math.Abs(x) + Math.Abs(z));
                        if (toSend != null) {
                            chunkCoords = toSend.Coords;
                            if (_loadedChunks.Add(chunkCoords)) {
                                world.AddListener(chunkCoords, this);
                                SendPacket(toSend.ToChunkPacket());
                                toSend.SendAllBlockEntities(this);
                            }
                        }
                        else {
                            _sentAllChunks = false;
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
                List<Tuple<int, ItemStack?>> dirtyItems = Container.GetDirtyItemStacks();
                if (dirtyVariables.Length > 1 || dirtyItems.Count > 0) {
                    SendPacket(new CbUpdateContainerPacket(Container.Id, dirtyVariables, dirtyItems));
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

    public IItemContainer? GetContainerFromId(uint id){
        if (id == 0)
            return Entity?.Inventory;
        if (Container != null && Container.Id == id)
            return Container.Container;
        return null;
    }
}