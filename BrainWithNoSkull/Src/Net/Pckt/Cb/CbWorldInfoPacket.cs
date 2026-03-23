using BrainWithNoSkull.Client.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbWorldInfoPacket : IPacket {

    public WorldSettings Settings;
    
    public CbWorldInfoPacket(){}
    
    public CbWorldInfoPacket(WorldSettings settings){
        Settings = settings;
    }
    
    public PacketType GetPacketType(){
        return PacketType.WorldInfo;
    }

    public void Write(PacketWriter writer){
        Settings.Write(writer);
    }

    public void Read(PacketReader reader){
        Settings = WorldSettings.Read(reader);
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandeWorldInfo(this);
    }
}