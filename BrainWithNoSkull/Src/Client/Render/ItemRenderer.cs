using BrainWithNoSkull.Client.Models.Items;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render;

public class ItemRenderer {
    private readonly IRenderEngine _renderEngine;
    private readonly GameClient _client;
    
    public ItemRenderer(GameClient client, IRenderEngine renderEngine){
        _client = client;
        _renderEngine = renderEngine;
    }

    public void RenderItemInGui(VertexBuilder vb, ItemStack stack, float x, float y, float z, float width, float height){
        Vector2D<float> screenPos = _client.GetScreenPos(x, y);

        IBakedItemModel model = ItemModels.BakedModels[stack.Item];
        
        Vector2D<float> size = _client.GetScreenSize(1, 1);
        ItemModelTransform transform = model.GetItemModelTransform(ItemModelTransformType.Gui);
        Matrix4X4<float> finalTransform = transform.Transform * Matrix4X4.CreateScale(size.X, size.Y, 0.001f);
        finalTransform *= Matrix4X4.CreateTranslation(screenPos.X, screenPos.Y, z);
        foreach (Vertex v in model.GetVertices(stack)) {
            vb.Add(v with{
                Pos = Vector3D.Transform(v.Pos, finalTransform),
                Normal = Vector3D.Normalize(Vector3D.Transform(v.Normal, transform.Rotation))
            });
        }
    }

    public void RenderItemInGuiWithCount(VertexBuilder vb, ItemStack stack, float x, float y, float z, FontRenderer font){
        RenderItemInGui(vb, stack, x, y, z, 16, 16);
        if (stack.Count > 1) {
            font.RenderString(x - 9, y - 9, z - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
        }
    }

    public void RenderItemInWorld(VertexBuilder vb, MatrixStack ms, ItemStack stack){
        IBakedItemModel model = ItemModels.BakedModels[stack.Item];
        
        ItemModelTransform transform = model.GetItemModelTransform(ItemModelTransformType.World);
        Matrix4X4<float> posTransform = transform.Transform * ms.GetLast();
        Matrix4X4<float> normTransform = transform.Rotation * ms.GetLast();
        
        foreach (Vertex v in model.GetVertices(stack)) {
            vb.Add(v with{
                Pos = Vector3D.Transform(v.Pos, posTransform),
                Normal = Vector3D.Transform(v.Normal, normTransform)
            });
        }
    }
}