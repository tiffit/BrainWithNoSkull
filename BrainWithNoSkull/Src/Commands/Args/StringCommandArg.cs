using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Commands.Components;

namespace BrainWithNoSkull.Commands.Args;

public class StringCommandArg : CommandArg {

    public static StringCommandArg Word(){
        return new StringCommandArg();
    }

    public static string Get(CommandContext context, string key){
        return (context.Args[key] as string)!;
    }
    
    public override bool TryCapture(CommandInput input, [MaybeNullWhen(false)] out object str, out int numChars){
        str = null;
        numChars = 0;
        if (input.RemainingLength() <= 0) {
            return false;
        }

        string rest = input.Peek(input.RemainingLength());

        int index = rest.IndexOf(' ');
        if (index == -1) {
            str = rest;
            numChars = rest.Length;
        }
        else {
            str = rest[..index];
            numChars = index;
        }

        return true;
    }
    
}