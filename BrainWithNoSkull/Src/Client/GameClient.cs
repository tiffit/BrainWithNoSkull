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
    private PlayerController? _playerController;

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
            Position = Vector3D<float>.Zero,
            Target = new Vector3D<float>(0, 0, 1)
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

        if (_playerController != null) {
            _playerController.Update(delta);
            _camera.Position = _serverConnection.PlayerEntity!.Position;
            _camera.Target = _camera.Position + _playerController.LookDirection;
        }
        
        _camera.Frustum = new Frustum(_camera);

        RenderEngine.OnRender(delta);
    }

    public void CreatePlayerController(ClientPlayerEntity player){
        _playerController = new PlayerController(this, player, _inputHandler);
    }

    private void Tick(double delta){
        while (_tickActions.TryDequeue(out Action? tickAction)) {
            tickAction.Invoke();
        }
        
        World?.Tick(delta);
        _serverConnection.Tick(delta);
        _server.Tick(delta);
    }
}