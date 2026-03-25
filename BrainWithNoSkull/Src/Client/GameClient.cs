using System.Collections.Concurrent;
using BrainWithNoSkull.Client.Net;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using Silk.NET.Input;
using Silk.NET.Input.Glfw;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;

namespace BrainWithNoSkull.Client;

public class GameClient {
    public static GameClient Instance{ get; private set; } = null!;

    public IRenderEngine RenderEngine{ get; private set; } = null!;
    public IWindow GameWindow{ get; private set; } = null!;

    private InputHandler _inputHandler = null!;
    private Camera _camera = null!;
    private float _pitch;
    private float _yaw;
    private Vector3D<float> _playerPos = new Vector3D<float>(0f, 130f, 0f);
    private bool _hasMouseMoved;

    private LocalGameServer _server;

    public ClientWorld? World;
    private ServerConnectionBase _serverConnection;

    private ConcurrentQueue<Action> _tickActions = [];
    private ConcurrentQueue<Action> _renderActions = [];

    public GameClient(){
        Instance = this;
    }

    public void Start(){
        Registries.Registries.RegisterAll();
        PacketRegistry.Register();

        SetupWindow();

        _camera = new Camera{
            FoV = 90,
            Position = _playerPos,
            Target = new Vector3D<float>(0, 0, 0)
        };

        _server = new LocalGameServer();
        LocalServerConnection localServerConnection = new LocalServerConnection(_server);
        localServerConnection.Connect();
        _serverConnection = localServerConnection;

        RenderEngine = VkRenderEngine.Instance;
        RenderEngine.Init(GameWindow, _camera);
        GameWindow.Render += DoRender;
        GameWindow.Update += Tick;
        GameWindow.Run();
        RenderEngine.Cleanup();
    }

    private void SetupWindow(){
        WindowOptions windowOptions = WindowOptions.DefaultVulkan with{
            Size = new Vector2D<int>(800, 600),
            Title = "a"
        };
        GlfwWindowing.Use();
        GlfwInput.RegisterPlatform();
        GameWindow = Window.Create(windowOptions);
        GameWindow.UpdatesPerSecond = 20;
        GameWindow.Load += OnWindowLoad;
        GameWindow.Initialize();
    }

    private void OnWindowLoad(){
        IInputContext ctx = GameWindow.CreateInput();
        _inputHandler = new InputHandler(this, ctx);
    }

    private void DoRender(double delta){
        RenderEngine.PreRender(delta);
        
        while (_renderActions.TryDequeue(out Action? renderAction)) {
            renderAction.Invoke();
        }

        World?.Render(delta);

        _inputHandler.Tick(delta, out Vector2D<float> mouseMovement);
        if (!_hasMouseMoved && mouseMovement.Equals(Vector2D<float>.Zero)) {
            _hasMouseMoved = true;
            mouseMovement = Vector2D<float>.Zero;
        }

        _pitch = Math.Clamp(_pitch + mouseMovement.Y / 300f, -MathF.PI / 2 + 0.0001f, MathF.PI / 2 - 0.0001f);
        _yaw = (_yaw - mouseMovement.X / 300f) % MathF.Tau;

        Vector3D<float> lookDirection = Vector3D.Normalize(
            Vector3D.Transform(new Vector3D<float>(0, 0, 1), Matrix4X4.CreateFromYawPitchRoll(_yaw, _pitch, 0))
        );

        float movementSpeed = 30f * (float)delta;
        Vector3D<float> lookHorizontal = Vector3D.Normalize(new Vector3D<float>(lookDirection.X, 0, lookDirection.Z));
        Vector3D<float> lookCross = Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, lookHorizontal));

        if (_inputHandler.IsKeyDown(Key.A)) {
            _playerPos += lookCross * movementSpeed;
        }

        if (_inputHandler.IsKeyDown(Key.D)) {
            _playerPos -= lookCross * movementSpeed;
        }

        if (_inputHandler.IsKeyDown(Key.W)) {
            _playerPos += lookHorizontal * movementSpeed;
        }

        if (_inputHandler.IsKeyDown(Key.S)) {
            _playerPos -= lookHorizontal * movementSpeed;
        }

        if (_inputHandler.IsKeyDown(Key.Space)) {
            _playerPos += new Vector3D<float>(0, 1, 0) * movementSpeed;
        }

        if (_inputHandler.IsKeyDown(Key.ShiftLeft)) {
            _playerPos -= new Vector3D<float>(0, 1, 0) * movementSpeed;
        }

        // if (World != null) {
        //     float worldSize = World.Settings.WorldSize * 16;
        //     if (_playerPos.X < 0) {
        //         _playerPos.X = worldSize - _playerPos.X;
        //     }else if (_playerPos.X > worldSize) {
        //         _playerPos.X -= worldSize;
        //     }
        //     if (_playerPos.Z < 0) {
        //         _playerPos.Z = worldSize - _playerPos.Z;
        //     }else if (_playerPos.Z > worldSize) {
        //         _playerPos.Z -= worldSize;
        //     }   
        // }

        _camera.Position = _playerPos;
        _camera.Target = _playerPos + lookDirection;
        _camera.Frustum = new Frustum(_camera);

        RenderEngine.OnRender(delta);
    }

    private void Tick(double delta){
        while (_tickActions.TryDequeue(out Action? tickAction)) {
            tickAction.Invoke();
        }
        
        _serverConnection.PlayerEntity?.SetPosition(_playerPos);
        World?.Tick(delta);
        _serverConnection.Tick(delta);
        _server.Tick(delta);
    }
}