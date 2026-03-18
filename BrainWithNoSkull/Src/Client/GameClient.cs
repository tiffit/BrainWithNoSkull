using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Util;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client;

public class GameClient {
    public static GameClient Instance{ get; private set; } = null!;

    public IRenderEngine RenderEngine{ get; private set; } = null!;
    public IWindow GameWindow{ get; private set; } = null!;

    public GameClient(){
        Instance = this;
    }

    public void Start(){
        SetupWindow();
        RenderEngine = VkRenderEngine.Instance;
        RenderEngine.Init(GameWindow);
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
        GameWindow.Initialize();
    }

    private void DoRender(double delta){
        RenderEngine.PreRender(delta);

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
            new Vector2D<float>(0, 1)
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