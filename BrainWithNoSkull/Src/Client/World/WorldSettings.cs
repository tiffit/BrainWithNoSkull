using BrainWithNoSkull.Net;

namespace BrainWithNoSkull.Client.World;

public class WorldSettings {

    public uint WorldSize = 8;
    public long Seed = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public void Write(PacketWriter writer){
        writer.Write(WorldSize);
    }

    public static WorldSettings Read(PacketReader reader){
        uint worldSize = reader.ReadUInt32();
        return new WorldSettings{
            WorldSize = worldSize
        };
    }

}