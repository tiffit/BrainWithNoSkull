using BrainWithNoSkull.Blocks;
using BrainWithNoSkull.Client.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbWorldInfoPacket : IPacket {

    private WorldSettings _settings;
    
    public CbWorldInfoPacket(){}
    
    public CbWorldInfoPacket(WorldSettings settings){
        _settings = settings;
    }
    
    public PacketType GetPacketType(){
        return PacketType.WorldInfo;
    }

    public void Write(PacketWriter writer){
        _settings.Write(writer);
    }

    public void Read(PacketReader reader){
        _settings = WorldSettings.Read(reader);
    }
}