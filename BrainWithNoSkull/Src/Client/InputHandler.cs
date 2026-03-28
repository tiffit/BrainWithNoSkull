using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client;

public class InputHandler {

    private readonly GameClient _client;
    private readonly IInputContext _context;

    private Vector2D<float> _mousePosition;
    private Vector2D<float>? _previousMousePositon;

    public readonly HashSet<Key> _pressedKeys = [];
    public readonly HashSet<MouseButton> _mouseDownThisFrame = [];
    
    public InputHandler(GameClient client, IInputContext context){
        _client = client;
        _context = context;
        
        foreach (IMouse mouse in _context.Mice) {
            mouse.MouseMove += OnMouseMove;
            mouse.MouseDown += OnMouseDown;
            mouse.Cursor.CursorMode = CursorMode.Disabled;
        }
        
        foreach (IKeyboard keyboard in _context.Keyboards) {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }
    }

    private void OnMouseMove(IMouse mouse, Vector2 amount){
        _mousePosition = amount.ToGeneric();
        _previousMousePositon ??= _mousePosition;
    }
    
    private void OnMouseDown(IMouse mouse, MouseButton btn){
        _mouseDownThisFrame.Add(btn);
    }
    
    private void OnKeyDown(IKeyboard keyboard, Key key, int scanCode){
        _pressedKeys.Add(key);
    }
    
    private void OnKeyUp(IKeyboard keyboard, Key key, int scanCode){
        _pressedKeys.Remove(key);
    }

    public void Tick(double delta, out Vector2D<float> mouseMovement){
        mouseMovement = _mousePosition - (_previousMousePositon ?? _mousePosition);
        _previousMousePositon = _mousePosition;
    }

    public void Reset(){
        _mouseDownThisFrame.Clear();
    }

    public bool IsKeyDown(Key key){
        return _pressedKeys.Contains(key);
    }

    public bool IsButtonPressedThisFrame(MouseButton btn){
        return _mouseDownThisFrame.Contains(btn);
    }
    
}