using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Entities;

public class ItemStackEntityRenderer: EntityRenderer<ItemStackEntity> {
    
    protected override void RenderEntity(ItemStackEntity entity, MatrixStack stack, VertexBuilder vb, float partialTick){
        if (!ItemStack.IsNull(entity.Stack)) {
            GameClient client = GameClient.Instance;
            float time = (float) client.GameWindow.Time;
            
            float rotation = (MathF.PI/2f * time) % (MathF.PI * 2);
            float bobbing = 0f;//Math.Abs((time % 2f) - 1f) * 0.25f - 0.125f;
            
            stack.Scale(0.5f, 0.5f, 0.5f);
            stack.Rotate(Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitY, rotation));
            stack.Translate(entity.Position.X, entity.Position.Y + bobbing, entity.Position.Z);
            client.ItemRenderer.RenderItemInWorld(vb, stack, entity.Stack);
        }
    }
    
}