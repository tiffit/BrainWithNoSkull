// See https://aka.ms/new-console-template for more information

using BrainWithNoSkull.Client.Render.RenderEngine;

IRenderEngine engine = VkRenderEngine.Instance;
engine.Init();
engine.StartLoop();
engine.Cleanup();