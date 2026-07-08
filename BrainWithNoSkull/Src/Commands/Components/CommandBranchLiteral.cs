namespace BrainWithNoSkull.Commands.Components;

public class CommandBranchLiteral: CommandBranch {

    private readonly string _text;

    public CommandBranchLiteral(string text){
        _text = text;
    }

    public override bool Valid(CommandContext context, CommandInput input){
        return input.RemainingLength() >= _text.Length 
               && input.Peek(_text.Length).Equals(_text) 
               && (input.RemainingLength() - _text.Length == 0 || input.Peek(_text.Length + 1).EndsWith(' '));
    }

    public override bool Execute(CommandContext context, CommandInput input){
        input.Advance(_text.Length);
        if (input.RemainingLength() > 0) {
            input.Advance(1);
        }
        return base.Execute(context, input);
    }

    public override string ToString(){
        return _text;
    }
}