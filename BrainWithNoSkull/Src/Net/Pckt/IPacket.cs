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
    PlayerInventory,
    NewEntity,
    
    //Server-bound
    PlayerMove,
    BreakBlock,
    ItemInteract,
    ChangeHotbarSlot,
    MoveInventoryItems
}

public delegate IPacket PacketFactory();

public static class PacketRegistry {

    private static readonly Dictionary<PacketType, PacketFactory> Registry = new Dictionary<PacketType, PacketFactory>();

    public static void Register(){
        Registry[PacketType.WorldInfo] = () => new CbWorldInfoPacket();
        Registry[PacketType.Chunk] = () => new CbChunkPacket();
        Registry[PacketType.PlayerInfo] = () => new CbPlayerInfoPacket();
        Registry[PacketType.PlayerInventory] = () => new CbPlayerInventoryPacket();;
        Registry[PacketType.NewEntity] = () => new CbNewEntityPacket();
        
        Registry[PacketType.PlayerMove] = () => new SbPlayerMovePacket();
        Registry[PacketType.BreakBlock] = () => new SbPlayerBreakBlockPacket();
        Registry[PacketType.ItemInteract] = () => new SbItemInteractPacket();
        Registry[PacketType.ChangeHotbarSlot] = () => new SbChangeHotbarSlotPacket();
        Registry[PacketType.MoveInventoryItems] = () => new SbMoveInventoryItemsPacket();
    }

    public static IPacket GetPacket(PacketType type) => Registry[type]();

    public static IPacket DecodePacket(PacketReader reader){
        PacketType type = (PacketType)reader.ReadUInt16();
        IPacket packet = GetPacket(type);
        packet.Read(reader);
        return packet;
    }
}

public record PacketContext(ConnectedClientBase? Client, ServerConnectionBase? ServerConnection, IClientPacketHandler? ClientPacketHandler);