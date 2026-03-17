namespace BrainWithNoSkull.Render.Shader;

public class ShaderPrograms {

    public static void LoadShaderBytes(string name, out byte[] vert, out byte[] frag){
        vert = File.ReadAllBytes($"./Assets/Shaders/Compiled/{name}_vert.spv");
        frag = File.ReadAllBytes($"./Assets/Shaders/Compiled/{name}_frag.spv");
    }
    
}