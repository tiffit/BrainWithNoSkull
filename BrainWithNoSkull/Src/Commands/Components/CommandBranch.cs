using BrainWithNoSkull.Commands.Args;

namespace BrainWithNoSkull.Commands.Components;

public abstract class CommandBranch {

    private readonly List<CommandBranch> _branches = [];
    private CommandAction? _action;

    public CommandBranch Run(CommandAction action){
        _action = action;
        return this;
    }

    public CommandBranch Branch(CommandBranch branch){
        _branches.Add(branch);
        return this;
    }

    public static CommandBranchLiteral Literal(string literal){
        return new CommandBranchLiteral(literal);
    }
    
    public static CommandBranchArg Arg(string key, CommandArg arg){
        return new CommandBranchArg(key, arg);
    }
    
    public static CommandBranchLiteral Root(CommandRegistry registry, string root){
        return registry.Register(Literal(root));
    }

    public abstract bool Valid(CommandContext context, CommandInput input);

    public virtual bool Execute(CommandContext context, CommandInput input){
        if (input.RemainingLength() <= 0) {
            if (_action != null) {
                _action(context);
                return true;
            }
            return false;
        }
        foreach (CommandBranch branch in _branches) {
            if (branch.Valid(context, input) && branch.Execute(context, input)) {
                return true;
            }
        }
        return false;
    }

}