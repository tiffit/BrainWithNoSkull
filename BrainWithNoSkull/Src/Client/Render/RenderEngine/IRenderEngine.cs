using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Util;
using Silk.NET.Windowing;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public interface IRenderEngine {
    void Init(IWindow window, Camera camera);

    void PreRender(double delta);
    
    void OnRender(double delta);

    Mesh CreateMesh(ShaderType shaderType, ulong size);
    void WriteMesh(Mesh mesh, VertexBuilder builder, AABB? aabb, ref TransformPushConstant pushConstant);
    void DestroyMesh(Mesh mesh);

    VertexBuilder GetVertexBuilder(ShaderType shaderType);

    void Cleanup();
}