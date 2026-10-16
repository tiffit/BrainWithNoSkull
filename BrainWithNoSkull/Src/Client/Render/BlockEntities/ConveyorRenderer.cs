using BrainWithNoSkull.Client.Models.Blocks.Unbaked.Specific;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.World.BlockEntities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.BlockEntities;

public class ConveyorRenderer: BlockEntityRenderer<ConveyorBlockEntity> {
    
    protected override void RenderBlockEntity(ConveyorBlockEntity entity, MatrixStack stack, VertexBuilder vb, float partialTick){
        stack.Translate(entity.BlockPosition);
        stack.Translate(0, 4/8f, 0);

        float conveyorAnim = ((float)GameClient.Instance.GameWindow.Time) % 2;
        
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(0, 0, 0),
            Uv = new Vector2D<float>(0, 0 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(1, 0, 0),
            Uv = new Vector2D<float>(1, 0 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(1, 0, 1),
            Uv = new Vector2D<float>(1, 1 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
        
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(1, 0, 1),
            Uv = new Vector2D<float>(1, 1 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(0, 0, 1),
            Uv = new Vector2D<float>(0, 1 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
        vb.Add(stack, new Vertex{
            Pos = new Vector3D<float>(0, 0, 0),
            Uv = new Vector2D<float>(0, 0 + conveyorAnim),
            Color = 0xff_ff_ff_ff,
            TextureIndex = ConveyorBlockModel.Instance.ConveyorBelt,
            Normal = new Vector3D<float>(0, 1, 0)
        });
    }
    
}