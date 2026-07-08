namespace BrainWithNoSkull.Commands.Components;

public record CommandContext(ICommandExecutor Executor, Dictionary<string, object> Args) {
    
}

public delegate void CommandAction(CommandContext context);