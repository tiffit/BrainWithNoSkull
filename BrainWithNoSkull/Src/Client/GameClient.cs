using System.Collections.Concurrent;
using BrainWithNoSkull.Client.Guis;
using BrainWithNoSkull.Client.Input;
using BrainWithNoSkull.Client.Net;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
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

    public InputHandler InputHandler = null!;
    private Camera _camera = null!;
    private readonly LightData _lightData = new LightData();
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
    public Gui? Gui;
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
        InputHandler = new InputHandler(this, ctx);
    }

    private void OnWindowResize(Vector2D<int> newSize){
        ScaledWindowSize = newSize / GuiScale;
        ReInitGuis();
    }

    public void ReInitGuis(){
        Hud?.Init(RenderEngine, FontRenderer, ScaledWindowSize.X, ScaledWindowSize.Y);
        Gui?.Init(RenderEngine, FontRenderer, ScaledWindowSize.X, ScaledWindowSize.Y);
    }

    private void DoRender(double delta){
        RenderEngine.PreRender(delta);

        if (ServerConnection.PlayerEntity != null) {
            UpdateSun(ServerConnection.PlayerEntity, World!);
        }

        Skybox.Render(RenderEngine, _lightData);

        VertexBuilder guiVb = RenderEngine.GetVertexBuilder(ShaderType.Gui);
        Hud?.RenderBase(guiVb);
        Gui?.RenderBase(guiVb);

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

    public void SetGui(Gui? gui){
        Gui = gui;
        if (gui == null) {
            InputHandler.SetCursorMode(CursorMode.Disabled);
        }else {
            gui.Init(RenderEngine, FontRenderer, ScaledWindowSize.X, ScaledWindowSize.Y);
            InputHandler.SetCursorMode(CursorMode.Normal);
        }
    }

    public void CreatePlayerController(ClientPlayerEntity player){
        _playerController = new PlayerController(this, player, InputHandler);
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
        Matrix4X4<float> mat = Matrix4X4.CreateRotationZ(MathF.Cos(playerPosX * 2 * MathF.PI) * MathF.PI / 2);
        return Vector3D.Transform(Vector3D<float>.UnitY, mat);
    }

    private Vector3D<float> GetSunSkyProgress(float playerPosZ, Vector3D<float> currentPos){
        Matrix4X4<float> mat = Matrix4X4.CreateRotationX(playerPosZ * MathF.PI * 2 + MathF.PI);
        return Vector3D.Transform(currentPos, mat);
    }

    private void DrawBlockOutline(WorldRayResult result){
        Vector3D<int> blockPos = result.Pos;
        AABB? aabb = World?.GetBlock(blockPos).GetCollisionBox();
        if (aabb == null)
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

        uint color = ColorUtil.FromFloats(0.8f, 0.8f, 0.8f);
        uint breakColor = ColorUtil.FromFloats(0.5f, 0.5f, 0.5f, 0.2f);

        VertexBuilder vbLines = RenderEngine.GetVertexBuilder(ShaderType.Lines);
        vbLines.Add(new Vertex{ Pos = p000, Color = color });
        vbLines.Add(new Vertex{ Pos = p100, Color = color });

        vbLines.Add(new Vertex{ Pos = p000, Color = color });
        vbLines.Add(new Vertex{ Pos = p010, Color = color });

        vbLines.Add(new Vertex{ Pos = p000, Color = color });
        vbLines.Add(new Vertex{ Pos = p001, Color = color });


        vbLines.Add(new Vertex{ Pos = p111, Color = color });
        vbLines.Add(new Vertex{ Pos = p011, Color = color });

        vbLines.Add(new Vertex{ Pos = p111, Color = color });
        vbLines.Add(new Vertex{ Pos = p101, Color = color });

        vbLines.Add(new Vertex{ Pos = p111, Color = color });
        vbLines.Add(new Vertex{ Pos = p110, Color = color });


        vbLines.Add(new Vertex{ Pos = p110, Color = color });
        vbLines.Add(new Vertex{ Pos = p010, Color = color });

        vbLines.Add(new Vertex{ Pos = p110, Color = color });
        vbLines.Add(new Vertex{ Pos = p100, Color = color });

        vbLines.Add(new Vertex{ Pos = p010, Color = color });
        vbLines.Add(new Vertex{ Pos = p011, Color = color });

        vbLines.Add(new Vertex{ Pos = p100, Color = color });
        vbLines.Add(new Vertex{ Pos = p101, Color = color });

        vbLines.Add(new Vertex{ Pos = p101, Color = color });
        vbLines.Add(new Vertex{ Pos = p001, Color = color });

        vbLines.Add(new Vertex{ Pos = p001, Color = color });
        vbLines.Add(new Vertex{ Pos = p011, Color = color });
        
        VertexBuilder vbBreakProgress = RenderEngine.GetVertexBuilder(ShaderType.WorldPosColor);
        ClientPlayerEntity? player = ServerConnection.PlayerEntity;
        if (player?.BlockBreakPosition != null) {
            Block block = player.World!.GetBlock(player.BlockBreakPosition.Value);
            float progress = float.Clamp(player.BlockBreakTime / block.BreakTime, 0, 1f);

            float centerX = (aabb.MaxX + aabb.MinX)/2;
            float centerY = (aabb.MaxY + aabb.MinY)/2;
            float centerZ = (aabb.MaxZ + aabb.MinZ)/2;

            float sizeX = (aabb.MaxX - aabb.MinX)/2 * progress;
            float sizeY = (aabb.MaxY - aabb.MinY)/2 * progress;
            float sizeZ = (aabb.MaxZ - aabb.MinZ)/2 * progress;
            
            //Up
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MaxY + padding, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MaxY + padding, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MaxY + padding, centerZ + sizeZ), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MaxY + padding, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MaxY + padding, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MaxY + padding, centerZ + sizeZ), Color = breakColor });
            
            //Down
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MinY - padding, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MinY - padding, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MinY - padding, centerZ + sizeZ), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, aabb.MinY - padding, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MinY - padding, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, aabb.MinY - padding, centerZ - sizeZ), Color = breakColor });
            
            //North (+x)
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY - sizeY, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY - sizeY, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY + sizeY, centerZ + sizeZ), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY - sizeY, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY + sizeY, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MaxX + padding, centerY + sizeY, centerZ - sizeZ), Color = breakColor });
            
            //South (-x)
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY - sizeY, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY + sizeY, centerZ + sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY - sizeY, centerZ + sizeZ), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY - sizeY, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY + sizeY, centerZ - sizeZ), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(aabb.MinX - padding, centerY + sizeY, centerZ + sizeZ), Color = breakColor });
            
            //East (+z)
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY - sizeY, aabb.MaxZ + padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY + sizeY, aabb.MaxZ + padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY - sizeY, aabb.MaxZ + padding), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY - sizeY, aabb.MaxZ + padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY + sizeY, aabb.MaxZ + padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY + sizeY, aabb.MaxZ + padding), Color = breakColor });
            
            //West (-z)
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY - sizeY, aabb.MinZ - padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY - sizeY, aabb.MinZ - padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY + sizeY, aabb.MinZ - padding), Color = breakColor });
            
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY - sizeY, aabb.MinZ - padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX + sizeX, centerY + sizeY, aabb.MinZ - padding), Color = breakColor });
            vbBreakProgress.Add(new Vertex{ Pos = new Vector3D<float>(centerX - sizeX, centerY + sizeY, aabb.MinZ - padding), Color = breakColor });
        }
    }
}