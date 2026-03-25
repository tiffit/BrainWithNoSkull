namespace BrainWithNoSkull.Client.Render.Shader;

[Flags]
public enum ShaderFlag {
    None = 0b0,
    CastShadow = 0b10,
    Mesh = 0b100,
    Skybox = 0b1000
}