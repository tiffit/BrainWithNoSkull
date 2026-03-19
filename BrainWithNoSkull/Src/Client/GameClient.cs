using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Util;
using BrainWithNoSkull.Registries;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client;

public class GameClient {
    public static GameClient Instance{ get; private set; } = null!;

    public IRenderEngine RenderEngine{ get; private set; } = null!;
    public IWindow GameWindow{ get; private set; } = null!;

    private InputHandler _inputHandler;
    private Camera _camera;
    private float _pitch = 0;
    private float _yaw = 0;
    private Vector3D<float> _playerPos = new Vector3D<float>(0f, 0f, -2f);
    private bool _hasMouseMoved = false;

    public GameClient(){
        Instance = this;
    }

    public void Start(){
        Registries.Registries.RegisterAll();

        SetupWindow();

        _camera = new Camera{
            FoV = 90,
            Position = new Vector3D<float>(2f, 2f, -2f),
            Target = new Vector3D<float>(0, 0, 0)
        };

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
        GameWindow = Window.Create(windowOptions);
        GameWindow.UpdatesPerSecond = 30;
        GameWindow.Load += OnWindowLoad;
        GameWindow.Initialize();
    }

    private void OnWindowLoad(){
        IInputContext ctx = GameWindow.CreateInput();
        _inputHandler = new InputHandler(this, ctx);
    }

    private void DoRender(double delta){
        RenderEngine.PreRender(delta);
        _inputHandler.Tick(delta, out Vector2D<float> mouseMovement);
        if (!_hasMouseMoved && mouseMovement.Equals(Vector2D<float>.Zero)) {
            _hasMouseMoved = true;
            mouseMovement = Vector2D<float>.Zero;
        }

        _pitch = Math.Clamp(_pitch + mouseMovement.Y / 300f, -MathF.PI/2 + 0.0001f, MathF.PI/2 - 0.0001f);
        _yaw = (_yaw - mouseMovement.X / 300f) % MathF.Tau;

        Vector3D<float> lookDirection = Vector3D.Normalize(
            Vector3D.Transform(new Vector3D<float>(0, 0, 1), Matrix4X4.CreateFromYawPitchRoll(_yaw, _pitch, 0))
        );

        float movementSpeed = 3f * (float)delta;
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

        _camera.Position = _playerPos;
        _camera.Target = _playerPos + lookDirection;

        VertexBuilder builder = RenderEngine.GetVertexBuilder(ShaderType.Shader);
        MatrixStack stack = builder.MatrixStack;
        float min = -0.5f;
        float max = 0.5f;
        VertexHelper.AddQuad(builder,
            new Vector3D<float>(min, min, min),
            new Vector3D<float>(max, min, min),
            new Vector3D<float>(max, max, min),
            new Vector3D<float>(min, max, min),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(1, 0),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(0, 1)
        );

        VertexHelper.AddQuad(builder,
            new Vector3D<float>(min, min, max),
            new Vector3D<float>(min, max, max),
            new Vector3D<float>(max, max, max),
            new Vector3D<float>(max, min, max),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(0, 1),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(1, 0)
        );

        VertexHelper.AddQuad(builder,
            new Vector3D<float>(min, max, min),
            new Vector3D<float>(max, max, min),
            new Vector3D<float>(max, max, max),
            new Vector3D<float>(min, max, max),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(1, 0),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(0, 1),
            new Vector3D<float>(1, 0, 0)
        );

        VertexHelper.AddQuad(builder,
            new Vector3D<float>(min, min, min),
            new Vector3D<float>(min, min, max),
            new Vector3D<float>(max, min, max),
            new Vector3D<float>(max, min, min),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(0, 1),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(1, 0)
        );

        VertexHelper.AddQuad(builder,
            new Vector3D<float>(min, min, min),
            new Vector3D<float>(min, max, min),
            new Vector3D<float>(min, max, max),
            new Vector3D<float>(min, min, max),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(1, 0),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(0, 1)
        );

        VertexHelper.AddQuad(builder,
            new Vector3D<float>(max, min, min),
            new Vector3D<float>(max, min, max),
            new Vector3D<float>(max, max, max),
            new Vector3D<float>(max, max, min),
            new Vector2D<float>(0, 0),
            new Vector2D<float>(0, 1),
            new Vector2D<float>(1, 1),
            new Vector2D<float>(1, 0)
        );
        stack.PopMatrix();

        RenderEngine.OnRender(delta);
    }

    private void Tick(double delta){
    }
}