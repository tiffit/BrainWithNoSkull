using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbPlayerFlagsPacket : IPacket {

    public bool Infinite;
    
    public CbPlayerFlagsPacket(){}
    
    public CbPlayerFlagsPacket(PlayerEntity player){
        Infinite = player.InfiniteEnabled;
    }
    
    public PacketType GetPacketType(){
        return PacketType.PlayerFlags;
    }

    public void Write(PacketWriter writer){
        writer.Write(Infinite);
    }

    public void Read(PacketReader reader){
        Infinite = reader.ReadBoolean();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandlePlayerFlags(this);
    }
}