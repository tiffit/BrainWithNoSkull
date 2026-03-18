namespace BrainWithNoSkull.Client.Render.Shader;

public abstract class ShaderProgram {

    public readonly ShaderDetails Details;

    protected ShaderProgram(ShaderDetails details){
        Details = details;
    }
}