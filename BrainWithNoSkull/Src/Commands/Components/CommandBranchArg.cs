using BrainWithNoSkull.Commands.Args;

namespace BrainWithNoSkull.Commands.Components;

public class CommandBranchArg: CommandBranch {

    private readonly string _key;
    private readonly CommandArg _arg;

    public CommandBranchArg(string key, CommandArg arg){
        _key = key;
        _arg = arg;
    }

    public override bool Valid(CommandContext context, CommandInput input){
        if (_arg.TryCapture(input, out object? obj, out int numChars)) {
            context.Args[_key] = obj;
            context.Args[_key + "__c"] = numChars;
            return true;
        }
        return false;
    }

    public override bool Execute(CommandContext context, CommandInput input){
        input.Advance((context.Args[_key + "__c"] as int?)!.Value);
        if (input.RemainingLength() > 0) {
            input.Advance(1);
        }
        return base.Execute(context, input);
    }

    public override string ToString(){
        return _key;
    }
}