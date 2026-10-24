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
            float bobbing = Math.Abs((time % 2f) - 1f) * 0.25f - 0.125f;
            Vector3D<float> pos = Vector3D.Lerp(entity.OldPosition, entity.Position, partialTick);
            
            stack.Translate(pos.X, pos.Y + bobbing, pos.Z);
            stack.Rotate(Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitY, rotation));
            stack.Scale(0.5f, 0.5f, 0.5f);
            client.ItemRenderer.RenderItemInWorld(vb, stack, entity.Stack);
        }
    }
    
}