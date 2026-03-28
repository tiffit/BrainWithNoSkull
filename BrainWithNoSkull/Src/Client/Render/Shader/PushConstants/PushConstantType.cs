using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.Render.Shader.PushConstants;

public unsafe record PushConstantType(ShaderStageFlags Stage, uint Size) {
    public static readonly PushConstantType None = new PushConstantType(ShaderStageFlags.None, 0);
    public static readonly PushConstantType Transform = new PushConstantType(ShaderStageFlags.VertexBit, TransformPushConstant.SizeInBytes);
    public static readonly PushConstantType Skybox = new PushConstantType(ShaderStageFlags.FragmentBit, (uint)sizeof(SkyboxPushConstant));
}