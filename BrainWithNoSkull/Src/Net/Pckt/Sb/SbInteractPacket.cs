using BrainWithNoSkull.Server;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbInteractPacket : IPacket {

    public Vector3D<int> Pos;
    
    public SbInteractPacket(){ }
    
    public SbInteractPacket(Vector3D<int> pos){
        Pos = pos;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Interact;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(Pos);
    }

    public void Read(PacketReader reader){
        Pos = reader.Read3DIntVec();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;
        if (player?.World != null) {
            if (player.World.GetBlock(Pos) is IBlockInteractable interactable) {
                if (interactable.CanInteract(player.World, player, Pos)) {
                    interactable.Interact((player.World as ServerWorld)!, player, Pos);
                }
            }
        }

    }
}