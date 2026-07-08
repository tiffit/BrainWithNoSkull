using BrainWithNoSkull.Commands.Args;
using BrainWithNoSkull.Commands.Components;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Commands;

public class GiveCommand {
    public static void Init(CommandRegistry registry){
        CommandBranch.Root(registry, "give")
            .Branch(
                CommandBranch.Arg("item", StringCommandArg.Word()).Run(RunGive)
                    .Branch(
                        CommandBranch.Arg("amount", IntCommandArg.New()).Run(RunGive)
                    )
            );
    }

    private static void RunGive(CommandContext context){
        Item? item = ItemRegistry.Registry.GetEntry(StringCommandArg.Get(context, "item"));
        if (item == null) {
            Console.WriteLine("Unknown item " + StringCommandArg.Get(context, "item"));
            return;
        }

        ItemStack? stack = new ItemStack(item, IntCommandArg.Get(context, "amount", 1));
        if (context.Executor is ServerPlayerEntity player) {
            (player.Inventory as IItemContainer).AddItem(ref stack);
        }
    }
}