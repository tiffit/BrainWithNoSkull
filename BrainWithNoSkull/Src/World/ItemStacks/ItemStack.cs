using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Items;

namespace BrainWithNoSkull.World.ItemStacks;

public class ItemStack {

    public Item Item{ get; private set; }
    public int Count;

    public ItemStack(Item item, int count){
        Item = item;
        Count = count;
    }

    public ItemStack Clone(){
        return new ItemStack(Item, Count);
    }

    public void Write(PacketWriter writer){
        writer.Write(Item.GetIndex());
        writer.Write((short)Count);
    }

    public static ItemStack Read(PacketReader reader){
        ushort itemIndex = reader.ReadUInt16();
        Item item = ItemRegistry.Registry.GetEntry(itemIndex)!;
        int count = reader.ReadInt16();
        return new ItemStack(item, count);
    }

    public static bool IsNull([NotNullWhen(false)] ItemStack? stack){
        return stack == null || stack.Count == 0 || stack.Item == ItemRegistry.BlockItems[BlockRegistry.Air];
    }

}