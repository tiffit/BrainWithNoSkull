using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbMoveInventoryItemsPacket : IPacket {

    public uint SrcContainer, DstContainer;
    public int SrcSlot, DstSlot;
    
    public SbMoveInventoryItemsPacket(){ }
    
    public SbMoveInventoryItemsPacket(uint srcContainer, int srcSlot, uint dstContainer, int dstSlot){
        SrcContainer = srcContainer;
        SrcSlot = srcSlot;
        DstContainer = dstContainer;
        DstSlot = dstSlot;
    }
    
    public PacketType GetPacketType(){
        return PacketType.MoveInventoryItems;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(SrcContainer);
        writer.Write(SrcSlot);
        writer.Write(DstContainer);
        writer.Write(DstSlot);
    }

    public void Read(PacketReader reader){
        SrcContainer = reader.ReadUInt32();
        SrcSlot = reader.ReadInt32();
        DstContainer = reader.ReadUInt32();
        DstSlot = reader.ReadInt32();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;
        if (player?.World != null) {
            IItemContainer? srcContainer = ctx.Client.GetContainerFromId(SrcContainer);
            IItemContainer? dstContainer = ctx.Client.GetContainerFromId(DstContainer);
            if (srcContainer != null && dstContainer != null) {
                ItemStack? srcStack = srcContainer.GetItem(SrcSlot);
                if (dstContainer.CombineStacks(DstSlot, ref srcStack, true)) {
                    srcContainer.SetItemInSlot(srcStack, SrcSlot);
                }
            }
        }

    }
}