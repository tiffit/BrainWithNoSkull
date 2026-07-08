using BrainWithNoSkull.Server;

namespace BrainWithNoSkull.Commands.Components;

public class CommandRegistry {

    private readonly GameServerBase _server;
    private readonly List<CommandBranchLiteral> _roots = [];
    
    public CommandRegistry(GameServerBase server){
        _server = server;
    }

    public void Init(){
        GiveCommand.Init(this);
    }

    public CommandBranchLiteral Register(CommandBranchLiteral branch){
        _roots.Add(branch);
        return branch;
    }

    public void Execute(ICommandExecutor executor, CommandInput input){
        CommandContext context = new CommandContext(executor, new Dictionary<string, object>());
        foreach (CommandBranchLiteral branch in _roots) {
            input.Reset();
            if (branch.Valid(context, input)) {
                Console.WriteLine("Executing command " + branch);
                branch.Execute(context, input);
            }
        }
    }
}