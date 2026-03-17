// See https://aka.ms/new-console-template for more information

using BrainWithNoSkull.Render.RenderEngine;

IRenderEngine engine = VkRenderEngine.Instance;
engine.Init();
engine.StartLoop();
engine.Cleanup();