using BrainWithNoSkull.Client.Models.Blocks;
using BrainWithNoSkull.Client.Models.Items;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Items;
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

        if (stack.Item is BlockItem blockItem) {
            Vector2D<float> size = _client.GetScreenSize(40, 40);
            Block block = blockItem.Block;
            IBakedBlockModel model = BlockModels.BakedModels[block];
            Quaternion<float> quat = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(1, 0, 0), float.DegreesToRadians(180+30));
            quat *= Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0, 1, 0), float.DegreesToRadians(45));
        
            Matrix4X4<float> rotMatrix = Matrix4X4.CreateFromQuaternion(quat);
            Matrix4X4<float> posMatrix = rotMatrix * Matrix4X4.CreateScale(size.X, size.Y, 0.001f);
            posMatrix *= Matrix4X4.CreateTranslation(screenPos.X, screenPos.Y, z);
            foreach (Vertex v in model.GetVertices(new Vector3D<float>(-0.5f, -0.5f, -0.5f), block, block.ConfigContainer.GetDefault(), [..Direction.All])) {
                vb.Add(v with{
                    Pos = Vector3D.Transform(v.Pos, posMatrix),
                    Normal = Vector3D.Normalize(Vector3D.Transform(v.Normal, rotMatrix))
                });
            }
        }
        else {
            Vector2D<float> size = _client.GetScreenSize(16*2, 16*2);
            IBakedItemModel model = ItemModels.BakedModels[stack.Item];
            Matrix4X4<float> posMatrix = Matrix4X4.CreateScale(size.X, size.Y, 0.001f);
            posMatrix *= Matrix4X4.CreateTranslation(screenPos.X, screenPos.Y, z);
            foreach (Vertex v in model.GetVertices(stack)) {
                vb.Add(v with{ Pos = Vector3D.Transform(v.Pos, posMatrix) });
            }
        }
    }

    public void RenderItemInGuiWithCount(VertexBuilder vb, ItemStack stack, float x, float y, float z, FontRenderer font){
        RenderItemInGui(vb, stack, x, y, z, 16, 16);
        if (stack.Count > 1) {
            font.RenderString(x - 9, y - 9, z - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
        }
    }

    public void RenderItemInWorld(VertexBuilder vb, MatrixStack ms, ItemStack stack){
        if (stack.Item is BlockItem item) {
            Block block = item.Block;
            IBakedBlockModel model = BlockModels.BakedModels[block];
        
            foreach (Vertex v in model.GetVertices(new Vector3D<float>(-0.5f, -0.5f, -0.5f), block, block.ConfigContainer.GetDefault(), [..Direction.All])) {
                vb.Add(v with{
                    Pos = ms.Transform(v.Pos)
                    // Normal = Vector3D.Normalize(ms.Transform(v.Normal))
                });
            }
        }
    }
}