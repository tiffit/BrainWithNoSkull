using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public interface IRenderEngine {
    void Init(IWindow window, Camera camera);

    void PreRender(double delta);
    
    void OnRender(double delta);

    Mesh CreateMesh(ShaderType shaderType, ulong size);
    Mesh CreateMeshCopy(Mesh other);
    void WriteMesh(Mesh mesh, VertexBuilder builder, ref TransformPushConstant pushConstant);
    void DestroyMesh(Mesh mesh);

    void Cleanup();
}