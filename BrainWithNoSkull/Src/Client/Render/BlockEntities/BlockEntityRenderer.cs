using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.BlockEntities;

namespace BrainWithNoSkull.Client.Render.BlockEntities;

public interface IBlockEntityRenderer {
    public void Render(BlockEntity blockEntity, MatrixStack stack, VertexBuilder vb, float partialTick);
}

public abstract class BlockEntityRenderer<T> : IBlockEntityRenderer where T : BlockEntity {

    protected abstract void RenderBlockEntity(T entity, MatrixStack stack, VertexBuilder vb, float partialTick);

    public void Render(BlockEntity blockEntity, MatrixStack stack, VertexBuilder vb, float partialTick){
        RenderBlockEntity((T)blockEntity, stack, vb, partialTick);
    }
}

public class BlockEntityRendererRegistry {
    private static readonly Dictionary<BlockEntityType, IBlockEntityRenderer> Renderers = new Dictionary<BlockEntityType, IBlockEntityRenderer>();

    public static void RegisterAll(){
        Renderers[BlockEntityRegistry.Conveyor] = new ConveyorRenderer();
    }
    
    public static IBlockEntityRenderer? GetRenderer(BlockEntityType type) {
        Renderers.TryGetValue(type, out IBlockEntityRenderer? renderer);
        return renderer;
    }
}