using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Input;

public enum InputType {
    Mouse,
    Keyboard
}

public struct BoundInput {
    public InputType InputType;
    public int Code;

    public BoundInput(Key code){
        InputType = InputType.Keyboard;
        Code = (int)code;
    }
    
    public BoundInput(MouseButton code){
        InputType = InputType.Mouse;
        Code = (int)code;
    }
}

[Flags]
public enum KeyContext {
    FirstPerson = 0b1,
    Gui = 0b10,
    All = FirstPerson | Gui
}

public class Keybinding {

    private readonly BoundInput _defaultInput;
    private readonly KeyContext _context;
    private readonly bool _isHeld;
    
    public Keybinding(string name, BoundInput defaultInput, KeyContext context, bool isHeld){
        _defaultInput = defaultInput;
        _context = context;
        _isHeld = isHeld;
    }

    public bool IsPressed(KeyContext context, InputHandler inputHandler){
        if (!_context.HasFlag(context))
            return false;
        return _defaultInput.InputType switch{
            InputType.Mouse => inputHandler.IsButtonPressedThisFrame((MouseButton)_defaultInput.Code),
            InputType.Keyboard => _isHeld ? inputHandler.IsKeyDown((Key)_defaultInput.Code) : inputHandler.IsKeyPressedThisFrame((Key)_defaultInput.Code),
            _ => false
        };
    }
    
}