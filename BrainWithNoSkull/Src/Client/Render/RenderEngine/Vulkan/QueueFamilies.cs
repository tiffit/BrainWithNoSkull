namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public struct QueueFamilies {
    public uint? Graphics;
    public uint? Present;
    public bool Complete => Graphics.HasValue && Present.HasValue;
}