using System.Collections.Concurrent;
using BrainWithNoSkull.Client.Atlas;
using BrainWithNoSkull.Client.Gui;
using BrainWithNoSkull.Client.Net;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Shader.PushConstants;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using Silk.NET.Input;
using Silk.NET.Input.Glfw;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;

namespace BrainWithNoSkull.Client;

public class GameClient {
    public static GameClient Instance{ get; private set; } = null!;

    public IRenderEngine RenderEngine{ get; private set; } = null!;
    public IWindow GameWindow{ get; private set; } = null!;

    private InputHandler _inputHandler = null!;
    private Camera _camera = null!;
    private LightData _lightData = new LightData();
    private PlayerController? _playerController;

    private LocalGameServer _server;

    public ClientWorld? World;
    public ServerConnectionBase ServerConnection;

    public ConcurrentQueue<Action> TickActions = [];
    public ConcurrentQueue<Action> RenderActions = [];

    public WorldRayResult? BlockRayResult;

    public int GuiScale = 3;
    public Vector2D<int> ScaledWindowSize;

    public HudGui? Hud;
    public FontRenderer FontRenderer;
    public ItemRenderer ItemRenderer;

    public GameClient(){
        Instance = this;
    }

    public void Start(){
        Registries.Registries.RegisterAll();
        PacketRegistry.Register();
        
        ClientAssetLoader.Load();

        SetupWindow();

        _camera = new Camera{
            FoV = 90,
            Position = Vector3D<float>.Zero,
            Target = new Vector3D<float>(0, 0, 1)
        };

        _server = new LocalGameServer();
        LocalServerConnection localServerConnection = new LocalServerConnection(_server);
        localServerConnection.Connect();
        ServerConnection = localServerConnection;

        RenderEngine = VkRenderEngine.Instance;
        RenderEngine.Init(GameWindow, _camera, _lightData);
        GameWindow.Render += DoRender;
        GameWindow.Update += Tick;
        
        FontRenderer = new FontRenderer(this, RenderEngine);
        ItemRenderer = new ItemRenderer(this, RenderEngine);
        Hud = new HudGui();
        Hud.Init(RenderEngine, FontRenderer, ScaledWindowSize.X, ScaledWindowSize.Y);
        
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
        GameWindow.FramesPerSecond = 100;
        GameWindow.Resize += OnWindowResize;
        GameWindow.Load += OnWindowLoad;
        GameWindow.Initialize();
        OnWindowResize(windowOptions.Size);
    }

    private void OnWindowLoad(){
        IInputContext ctx = GameWindow.CreateInput();
        _inputHandler = new InputHandler(this, ctx);
    }

    private void OnWindowResize(Vector2D<int> newSize){
        ScaledWindowSize = newSize / GuiScale;
        Hud?.Init(RenderEngine, FontRenderer, ScaledWindowSize.X, ScaledWindowSize.Y);
    }

    private void DoRender(double delta){
        RenderEngine.PreRender(delta);

        if (ServerConnection.PlayerEntity != null) {
            UpdateSun(ServerConnection.PlayerEntity, World!);
        }
        
        Skybox.Render(RenderEngine, _lightData);

        VertexBuilder guiVb = RenderEngine.GetVertexBuilder(ShaderType.Gui);
        Hud?.Render(guiVb);
        
        while (RenderActions.TryDequeue(out Action? renderAction)) {
            renderAction.Invoke();
        }

        World?.Render(delta);

        if (_playerController != null) {
            _playerController.Update(delta);
            _camera.Position = ServerConnection.PlayerEntity!.Position + new Vector3D<float>(0, 1.4f, 0);
            _camera.Target = _camera.Position + _playerController.LookDirection;

            WorldRay ray = new WorldRay(_camera.Position, _playerController.LookDirection, 5);
            BlockRayResult = ray.RunBlockTest(World!);
            if (BlockRayResult != null) {
                DrawBlockOutline(BlockRayResult.Value);
            }
        }
        
        _camera.Frustum = new Frustum(_camera);

        RenderEngine.OnRender(delta);
    }

    public void CreatePlayerController(ClientPlayerEntity player){
        _playerController = new PlayerController(this, player, _inputHandler);
    }

    public Vector2D<float> GetScreenPos(float x, float y){
        Vector2D<int> windowSize = GameWindow.Size;
        float screenX = x * GuiScale / windowSize.X * 2 - 1;
        float screenY = y * GuiScale / windowSize.Y * 2 - 1;
        return new Vector2D<float>(screenX, screenY);
    }
    
    public Vector2D<float> GetScreenSize(float w, float h){
        Vector2D<int> windowSize = GameWindow.Size;
        float screenX = w * GuiScale / windowSize.X / 2;
        float screenY = h * GuiScale / windowSize.Y / 2;
        return new Vector2D<float>(screenX, screenY);
    }

    private void Tick(double delta){
        while (TickActions.TryDequeue(out Action? tickAction)) {
            tickAction.Invoke();
        }
        
        World?.Tick(delta);
        ServerConnection.Tick(delta);
        _server.Tick(delta);
    }

    private void UpdateSun(ClientPlayerEntity player, ClientWorld world){
        Vector2D<float> playerPosMap = new Vector2D<float>(player.Position.X, player.Position.Z);
        playerPosMap /= (float)world.Settings.WorldSize * Constants.ChunkSize;
        _lightData.LightDir = Vector3D.Normalize(-GetSunSkyProgress(playerPosMap.Y, GetSunSkyTilt(playerPosMap.X)));
    }

    private Vector3D<float> GetSunSkyTilt(float playerPosX){
        Matrix4X4<float> mat = Matrix4X4.CreateRotationZ(MathF.Cos(playerPosX * 2 * MathF.PI) * MathF.PI/2);
        return Vector3D.Transform(Vector3D<float>.UnitY, mat);
    }
    
    private Vector3D<float> GetSunSkyProgress(float playerPosZ, Vector3D<float> currentPos){
        Matrix4X4<float> mat = Matrix4X4.CreateRotationX(playerPosZ*MathF.PI*2 + MathF.PI);
        return Vector3D.Transform(currentPos, mat);
    }

    private void DrawBlockOutline(WorldRayResult result){
        Vector3D<int> blockPos = result.Pos;
        AABB? aabb = World?.GetBlock(blockPos).GetCollisionBox();
        if(aabb == null)
            return;
        aabb += blockPos;
        const float padding = 0.001f;
        Vector3D<float> p000 = new Vector3D<float>(aabb.MinX - padding, aabb.MinY - padding, aabb.MinZ - padding);
        Vector3D<float> p111 = new Vector3D<float>(aabb.MaxX + padding, aabb.MaxY + padding, aabb.MaxZ + padding);
        
        Vector3D<float> p100 = new Vector3D<float>(p111.X, p000.Y, p000.Z);
        Vector3D<float> p010 = new Vector3D<float>(p000.X, p111.Y, p000.Z);
        Vector3D<float> p001 = new Vector3D<float>(p000.X, p000.Y, p111.Z);
        
        Vector3D<float> p011 = new Vector3D<float>(p000.X, p111.Y, p111.Z);
        Vector3D<float> p101 = new Vector3D<float>(p111.X, p000.Y, p111.Z);
        Vector3D<float> p110 = new Vector3D<float>(p111.X, p111.Y, p000.Z);

        Vector3D<float> color = new Vector3D<float>(0.8f, 0.8f, 0.8f);
        
        VertexBuilder builder = RenderEngine.GetVertexBuilder(ShaderType.Lines);
        builder.Add(new Vertex{ Pos = p000, Color = color });
        builder.Add(new Vertex{ Pos = p100, Color = color });
        
        builder.Add(new Vertex{ Pos = p000, Color = color });
        builder.Add(new Vertex{ Pos = p010, Color = color });
        
        builder.Add(new Vertex{ Pos = p000, Color = color });
        builder.Add(new Vertex{ Pos = p001, Color = color });
        
        
        builder.Add(new Vertex{ Pos = p111, Color = color });
        builder.Add(new Vertex{ Pos = p011, Color = color });
        
        builder.Add(new Vertex{ Pos = p111, Color = color });
        builder.Add(new Vertex{ Pos = p101, Color = color });
        
        builder.Add(new Vertex{ Pos = p111, Color = color });
        builder.Add(new Vertex{ Pos = p110, Color = color });
        
        
        builder.Add(new Vertex{ Pos = p110, Color = color });
        builder.Add(new Vertex{ Pos = p010, Color = color });
        
        builder.Add(new Vertex{ Pos = p110, Color = color });
        builder.Add(new Vertex{ Pos = p100, Color = color });
        
        builder.Add(new Vertex{ Pos = p010, Color = color });
        builder.Add(new Vertex{ Pos = p011, Color = color });
        
        builder.Add(new Vertex{ Pos = p100, Color = color });
        builder.Add(new Vertex{ Pos = p101, Color = color });
        
        builder.Add(new Vertex{ Pos = p101, Color = color });
        builder.Add(new Vertex{ Pos = p001, Color = color });
        
        builder.Add(new Vertex{ Pos = p001, Color = color });
        builder.Add(new Vertex{ Pos = p011, Color = color });
    }
}