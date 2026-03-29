namespace BrainWithNoSkull.Client.Render.Shader;

[Flags]
public enum ShaderFlag {
    None = 0b0,
    CastShadow = 0b10,
    Mesh = 0b100,
    Skybox = 0b1000,
    Lines = 0b1_0000,
    Gui = 0b10_0000,
    Font = 0b100_0000
}