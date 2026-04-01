using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Net.Pckt.Cb;
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
                                SendPacket(toSend.ToChunkPacket());
                            }
                        }
                    }
                }
            }

            if (!_playerInfoSent) {
                _playerInfoSent = true;
                SendPacket(new CbPlayerInfoPacket(Entity.Position));
            }

            if (Entity.Inventory.DirtySlots.Count > 0) {
                SendPacket(new CbPlayerInventoryPacket(Entity.Inventory));
                Entity.Inventory.DirtySlots.Clear();
            }
        }
    }
}