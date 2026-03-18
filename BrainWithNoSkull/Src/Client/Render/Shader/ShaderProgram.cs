namespace BrainWithNoSkull.Client.Render.Shader;

public abstract class ShaderProgram {

    public readonly ShaderType ShType;

    protected ShaderProgram(ShaderType shType){
        ShType = shType;
    }
}