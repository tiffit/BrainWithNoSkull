using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public interface IRenderEngine {
    void Init(IWindow window);

    void PreRender(double delta);
    
    void OnRender(double delta);

    VertexBuilder GetVertexBuilder(ShaderType shaderType);

    void Cleanup();
}