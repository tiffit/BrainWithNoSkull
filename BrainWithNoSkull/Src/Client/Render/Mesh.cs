using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.Client.Render;

public record Mesh(ShaderType SType, BufferAllocator.MemoryBlock Memory){
    
}

public record CompiledMeshData(uint VertexCount, AABB? Aabb, TransformPushConstant PushConstant) {
    
}