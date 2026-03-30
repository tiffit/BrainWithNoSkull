using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Client.Net;

public class ClientPacketHandler : IClientPacketHandler {

    private readonly ServerConnectionBase _serverConnection;
    private readonly GameClient _client;
    
    public ClientPacketHandler(ServerConnectionBase serverConnection, GameClient client){
        _serverConnection = serverConnection;
        _client = client;
    }

    public void HandeWorldInfo(CbWorldInfoPacket packet){
        _client.World = new ClientWorld(_client, _serverConnection, packet.Settings);
    }

    public void HandeChunk(CbChunkPacket packet){
        ClientWorld? world = _client.World;
        if (world != null) {
            Chunk chunk = new Chunk(world, packet.ChunkCoords, packet.Blocks);
            world.OnChunkLoaded(chunk);
        }
    }

    public void HandlePlayerInfo(CbPlayerInfoPacket packet){
        ClientWorld? world = _client.World;
        if (world != null) {
            ClientPlayerEntity player = new ClientPlayerEntity(_serverConnection, world);
            _serverConnection.PlayerEntity = player;
            world.AddPlayer(player, packet);
            _client.CreatePlayerController(player);
        }
    }

    public void HandlePlayerInventory(CbPlayerInventoryPacket packet){
        if (_serverConnection.PlayerEntity != null) {
            foreach ((short slot, ItemStack? stack) in packet.Slots) {
                _serverConnection.PlayerEntity.Inventory.SetItemInSlot(stack, slot);
            }
        }
    }
}