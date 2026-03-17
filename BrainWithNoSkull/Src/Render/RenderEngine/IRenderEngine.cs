namespace BrainWithNoSkull.Render.RenderEngine;

public interface IRenderEngine {
    void Init();

    void StartLoop();

    void Cleanup();
}