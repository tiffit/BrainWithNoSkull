using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Net.Pckt.Sb;

namespace BrainWithNoSkull.Net.Pckt;

public interface IPacket {
    
    PacketType GetPacketType();

    void Write(PacketWriter writer);
    void Read(PacketReader reader);

    void Handle(PacketContext ctx);

    public void EncodePacket(PacketWriter writer){
        writer.Write((ushort)GetPacketType());
        Write(writer);
    }
}

public enum PacketType: ushort{
    //Client-bound
    WorldInfo,
    Chunk,
    PlayerInfo,
    PlayerFlags,
    PlayerInventory,
    NewEntity,
    EntityMove,
    RemoveEntity,
    NewBlockEntity,
    OpenContainer,
    UpdateContainer,
    Block,
    FullLight,
    ConveyorSync,
    
    //Server-bound
    PlayerMove,
    BreakBlock,
    ItemUse,
    ChangeHotbarSlot,
    MoveInventoryItems,
    StartCraft,
    Interact,
    Chat
}

public delegate IPacket PacketFactory();

public static class PacketRegistry {

    private static readonly Dictionary<PacketType, PacketFactory> Registry = new Dictionary<PacketType, PacketFactory>();

    public static void Register(){
        //Client-bound
        Registry[PacketType.WorldInfo] = () => new CbWorldInfoPacket();
        Registry[PacketType.Chunk] = () => new CbChunkPacket();
        Registry[PacketType.PlayerInfo] = () => new CbPlayerInfoPacket();
        Registry[PacketType.PlayerFlags] = () => new CbPlayerFlagsPacket();
        Registry[PacketType.PlayerInventory] = () => new CbPlayerInventoryPacket();;
        Registry[PacketType.NewEntity] = () => new CbNewEntityPacket();
        Registry[PacketType.EntityMove] = () => new CbEntityMovePacket();
        Registry[PacketType.RemoveEntity] = () => new CbRemoveEntityPacket();
        Registry[PacketType.NewBlockEntity] = () => new CbNewBlockEntityPacket();
        Registry[PacketType.OpenContainer] = () => new CbOpenContainerPacket();
        Registry[PacketType.UpdateContainer] = () => new CbUpdateContainerPacket();
        Registry[PacketType.Block] = () => new CbBlockPacket();
        Registry[PacketType.FullLight] = () => new CbFullLightPacket();
        Registry[PacketType.ConveyorSync] = () => new CbConveyorSyncPacket();
        
        //Server-bound
        Registry[PacketType.PlayerMove] = () => new SbPlayerMovePacket();
        Registry[PacketType.BreakBlock] = () => new SbPlayerBreakBlockPacket();
        Registry[PacketType.ItemUse] = () => new SbItemUsePacket();
        Registry[PacketType.ChangeHotbarSlot] = () => new SbChangeHotbarSlotPacket();
        Registry[PacketType.MoveInventoryItems] = () => new SbMoveInventoryItemsPacket();
        Registry[PacketType.StartCraft] = () => new SbStartCraftPacket();
        Registry[PacketType.Interact] = () => new SbInteractPacket();
        Registry[PacketType.Chat] = () => new SbChatPacket();
    }

    public static IPacket GetPacket(PacketType type) => Registry[type]();

    public static IPacket DecodePacket(PacketReader reader){
        PacketType type = (PacketType)reader.ReadUInt16();
        IPacket packet = GetPacket(type);
        packet.Read(reader);
        return packet;
    }
}

public record PacketContext(ConnectedClientBase? Client, IClientPacketHandler? ClientPacketHandler);