using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Input;

public class InputHandler {

    private readonly GameClient _client;
    private readonly IInputContext _context;

    public Vector2D<float> MousePosition;
    private Vector2D<float>? _previousMousePositon;

    public readonly HashSet<Key> PressedKeys = [];
    public readonly HashSet<MouseButton> PressedMouseButtons = [];
    public readonly HashSet<MouseButton> MouseDownThisFrame = [];
    public readonly HashSet<Key> KeysDownThisFrame = [];
    public readonly HashSet<char> CharsDownThisFrame = [];
    
    public InputHandler(GameClient client, IInputContext context){
        _client = client;
        _context = context;
        
        foreach (IMouse mouse in _context.Mice) {
            mouse.MouseMove += OnMouseMove;
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
        }
        
        foreach (IKeyboard keyboard in _context.Keyboards) {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
            keyboard.KeyChar += OnKeyChar;
        }
        
        SetCursorMode(CursorMode.Disabled);
    }

    private void OnMouseMove(IMouse mouse, Vector2 amount){
        MousePosition = amount.ToGeneric();
        _previousMousePositon ??= MousePosition;
    }
    
    private void OnMouseDown(IMouse mouse, MouseButton btn){
        MouseDownThisFrame.Add(btn);
        PressedMouseButtons.Add(btn);
    }

    private void OnMouseUp(IMouse mouse, MouseButton btn){
        PressedMouseButtons.Remove(btn);
    }
    
    private void OnKeyDown(IKeyboard keyboard, Key key, int scanCode){
        PressedKeys.Add(key);
        KeysDownThisFrame.Add(key);
    }
    
    private void OnKeyUp(IKeyboard keyboard, Key key, int scanCode){
        PressedKeys.Remove(key);
    }
    
    private void OnKeyChar(IKeyboard keyboard, char chr){
        CharsDownThisFrame.Add(chr);
    }

    public void Tick(double delta, out Vector2D<float> mouseMovement){
        mouseMovement = MousePosition - (_previousMousePositon ?? MousePosition);
        _previousMousePositon = MousePosition;
    }

    public void Reset(){
        MouseDownThisFrame.Clear();
        KeysDownThisFrame.Clear();
        CharsDownThisFrame.Clear();
    }

    public bool IsKeyDown(Key key){
        return PressedKeys.Contains(key);
    }
    
    public bool IsMouseButtonDown(MouseButton key){
        return PressedMouseButtons.Contains(key);
    }

    public bool IsButtonPressedThisFrame(MouseButton btn){
        return MouseDownThisFrame.Contains(btn);
    }
    
    public bool IsKeyPressedThisFrame(Key key){
        return KeysDownThisFrame.Contains(key);
    }

    public void SetCursorMode(CursorMode mode){
        foreach (IMouse mouse in _context.Mice) {
            Vector2D<float> mousePos = (Vector2D<float>)_client.GameWindow.Size / 2;
            mouse.Position = mousePos.ToSystem();
            mouse.Cursor.CursorMode = mode;
            MousePosition = mousePos;
            _previousMousePositon = mousePos;
        }
    }
    
}