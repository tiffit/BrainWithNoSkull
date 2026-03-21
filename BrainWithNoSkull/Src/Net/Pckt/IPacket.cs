using BrainWithNoSkull.Net.Pckt.Cb;

namespace BrainWithNoSkull.Net.Pckt;

public interface IPacket {
    
    PacketType GetPacketType();

    void Write(PacketWriter writer);
    void Read(PacketReader reader);
}

public enum PacketType: ushort{
    WorldInfo,
    Chunk
}

public delegate IPacket PacketFactory();

public static class PacketRegistry {

    private static readonly Dictionary<PacketType, PacketFactory> Registry = new Dictionary<PacketType, PacketFactory>();

    public static void Register(){
        Registry[PacketType.WorldInfo] = () => new CbWorldInfoPacket();
        Registry[PacketType.Chunk] = () => new CbChunkPacket();
    }

    public static IPacket GetPacket(PacketType type) => Registry[type]();

}