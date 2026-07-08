using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Commands.Components;

namespace BrainWithNoSkull.Commands.Args;

public abstract class CommandArg {

    public abstract bool TryCapture(CommandInput input, [MaybeNullWhen(false)] out object obj, out int numChars);

}