using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Commands.Components;

namespace BrainWithNoSkull.Commands.Args;

public class IntCommandArg : CommandArg {

    public static IntCommandArg New(){
        return new IntCommandArg();
    }

    public static int Get(CommandContext context, string key, int def = 0){
        return context.Args.TryGetValue(key, out object? value) ? (value as int?)!.Value : def;
    }
    
    public override bool TryCapture(CommandInput input, [MaybeNullWhen(false)] out object val, out int numChars){
        val = null;
        numChars = 0;
        if (input.RemainingLength() <= 0) {
            return false;
        }

        string str = input.Peek(input.RemainingLength());

        int index = str.IndexOf(' ');
        if (index != -1) {
            str = str[..index];
        }

        if (int.TryParse(str, out int result)) {
            val = result;
            numChars = str.Length;
            return true;
        }
        return false;
    }
    
}