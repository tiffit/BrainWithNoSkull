using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.Recipes;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbStartCraftPacket : IPacket {

    public Recipe Recipe = null!;
    
    public SbStartCraftPacket(){ }
    
    public SbStartCraftPacket(Recipe recipe){
        Recipe = recipe;
    }
    
    public PacketType GetPacketType(){
        return PacketType.StartCraft;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(Recipe.GetIndex());
    }

    public void Read(PacketReader reader){
        Recipe = RecipeRegistry.Registry.GetEntry(reader.ReadUInt16())!;
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;

        if (player?.World != null) {
            PlayerInventory inv = player.Inventory;
            if ((player.InfiniteEnabled || Recipe.CanCraft(inv)) && Recipe.CanOutput(inv)) {
                if (!player.InfiniteEnabled) {
                    Recipe.ConsumeIngredients(inv);
                }
                Recipe.ProduceOutput(inv);
            }
        }
    }
}