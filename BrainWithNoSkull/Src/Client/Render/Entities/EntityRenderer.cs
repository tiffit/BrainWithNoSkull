using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities;

namespace BrainWithNoSkull.Client.Render.Entities;

public interface IEntityRenderer {
    public void Render(Entity entity, MatrixStack stack, VertexBuilder vb);
}

public abstract class EntityRenderer<T> : IEntityRenderer where T : Entity {

    protected abstract void RenderEntity(T entity, MatrixStack stack, VertexBuilder vb);

    public void Render(Entity entity, MatrixStack stack, VertexBuilder vb){
        RenderEntity((T)entity, stack, vb);
    }
}

public class EntityRendererRegistry {
    private static readonly Dictionary<EntityType, IEntityRenderer> Renderers = new Dictionary<EntityType, IEntityRenderer>();

    public static void RegisterAll(){
        Renderers[EntityTypeRegistry.ItemStack] = new ItemStackEntityRenderer();
    }
    
    public static IEntityRenderer? GetRenderer<T>(EntityType type) where T : Entity{
        Renderers.TryGetValue(type, out IEntityRenderer? renderer);
        return renderer;
    }
}