using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Input;

public class InputHandler {

    private readonly GameClient _client;
    private readonly IInputContext _context;

    public Vector2D<float> MousePosition;
    private Vector2D<float>? _previousMousePositon;

    private readonly HashSet<Key> _pressedKeys = [];
    private readonly HashSet<MouseButton> _mouseDownThisFrame = [];
    private readonly HashSet<Key> _keysDownThisFrame = [];
    
    public InputHandler(GameClient client, IInputContext context){
        _client = client;
        _context = context;
        
        foreach (IMouse mouse in _context.Mice) {
            mouse.MouseMove += OnMouseMove;
            mouse.MouseDown += OnMouseDown;
        }
        
        foreach (IKeyboard keyboard in _context.Keyboards) {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }
        
        SetCursorMode(CursorMode.Disabled);
    }

    private void OnMouseMove(IMouse mouse, Vector2 amount){
        MousePosition = amount.ToGeneric();
        _previousMousePositon ??= MousePosition;
    }
    
    private void OnMouseDown(IMouse mouse, MouseButton btn){
        _mouseDownThisFrame.Add(btn);
    }
    
    private void OnKeyDown(IKeyboard keyboard, Key key, int scanCode){
        _pressedKeys.Add(key);
        _keysDownThisFrame.Add(key);
    }
    
    private void OnKeyUp(IKeyboard keyboard, Key key, int scanCode){
        _pressedKeys.Remove(key);
    }

    public void Tick(double delta, out Vector2D<float> mouseMovement){
        mouseMovement = MousePosition - (_previousMousePositon ?? MousePosition);
        _previousMousePositon = MousePosition;
    }

    public void Reset(){
        _mouseDownThisFrame.Clear();
        _keysDownThisFrame.Clear();
    }

    public bool IsKeyDown(Key key){
        return _pressedKeys.Contains(key);
    }

    public bool IsButtonPressedThisFrame(MouseButton btn){
        return _mouseDownThisFrame.Contains(btn);
    }
    
    public bool IsKeyPressedThisFrame(Key key){
        return _keysDownThisFrame.Contains(key);
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