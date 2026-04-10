using BrainWithNoSkull.Client.Guis;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Entities;
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
            Chunk chunk = new Chunk(world, packet.ChunkCoords, packet.Blocks, packet.BlockConfigs, [], []);
            world.OnChunkLoaded(chunk);
        }
    }

    public void HandlePlayerInfo(CbPlayerInfoPacket packet){
        ClientWorld? world = _client.World;
        if (world != null) {
            ClientPlayerEntity player = new ClientPlayerEntity(_serverConnection, world){
                Id = packet.Id
            };
            _serverConnection.PlayerEntity = player;
            world.AddPlayer(player, packet);
            _client.CreatePlayerController(player);
            _client.ReInitGuis();
        }
    }

    public void HandlePlayerInventory(CbPlayerInventoryPacket packet){
        if (_serverConnection.PlayerEntity != null) {
            foreach ((short slot, ItemStack? stack) in packet.Slots) {
                _serverConnection.PlayerEntity.Inventory.SetItemInSlot(stack, slot);
            }
        }
    }

    public void HandleNewEntity(CbNewEntityPacket packet){
        WorldBase? world = _serverConnection.PlayerEntity?.World;
        if (world != null) {
            Entity entity = packet.EntityType.Factory(world);
            entity.Id = packet.Id;
            world.AddEntity(entity, packet.Position);
            using MemoryStream stream = new MemoryStream(packet.ExtraData);
            using PacketReader reader = new PacketReader(stream);
            entity.ReadFromPacket(reader);
        }
    }

    public void HandleEntityMove(CbEntityMovePacket packet){
        WorldBase? world = _serverConnection.PlayerEntity?.World;
        if (world != null) {
            Entity? entity = world.GetEntity(packet.Id);
            if (entity != null) {
                entity.SetPosition(packet.Position);
            }
        }
    }

    public void HandleRemoveEntity(CbRemoveEntityPacket packet){
        WorldBase? world = _serverConnection.PlayerEntity?.World;
        if (world != null) {
            Entity? entity = world.GetEntity(packet.Id);
            if (entity != null) {
                entity.Remove = true;
            }
        }
    }

    public void HandleNewBlockEntity(CbNewBlockEntityPacket packet){
        WorldBase? world = _serverConnection.PlayerEntity?.World;
        if (world != null) {
            BlockEntity entity = packet.BlockEntityType.Create();
            Chunk? chunk = world.GetChunk(ChunkUtils.ToChunkCoords(packet.Position));
            chunk?.SetBlockEntity(packet.Position.X, packet.Position.Y, packet.Position.Z, entity);
        }
    }

    public void HandleOpenContainer(CbOpenContainerPacket packet){
        DummyItemContainer container = new DummyItemContainer(packet.Id, packet.Size);
        ContainerSync sync = packet.Type.Factory(packet.Id, container, _serverConnection.PlayerEntity!);
        _serverConnection.Container = sync;
        if(_client.ContainerGuis.TryGetValue(packet.Type, out Func<ContainerSync, ContainerGui>? factory)) {
            _client.SetGui(factory(sync));
        }
    }

    public void HandleUpdateContainer(CbUpdateContainerPacket packet){
        ContainerSync? sync = _serverConnection.Container;
        if (sync != null && sync.Id == packet.Id) {
            sync.UpdateSyncVariables(packet.SyncVariables);
            foreach ((int slot, ItemStack? stack) in packet.UpdatedItems) {
                sync.Container.SetItemInSlot(stack, slot);
            }
        }
    }

    public void HandleBlock(CbBlockPacket packet){
        WorldBase? world = _serverConnection.PlayerEntity?.World;
        world?.SetBlock(packet.Position, packet.Block, packet.Config);
    }
}