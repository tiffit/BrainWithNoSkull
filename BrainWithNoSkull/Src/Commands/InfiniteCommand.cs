using BrainWithNoSkull.Commands.Components;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Commands;

public class InfiniteCommand {
    public static void Init(CommandRegistry registry){
        CommandBranch.Root(registry, "infinite").Run(RunInfinite);
    }

    private static void RunInfinite(CommandContext context){
        if (context.Executor is ServerPlayerEntity player) {
            player.InfiniteEnabled = true;
            player.UpdateFlags();
        }
    }
}