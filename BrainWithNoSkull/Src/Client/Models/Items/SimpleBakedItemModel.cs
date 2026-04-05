using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Client.Models.Items;

public interface IBakedItemModel {
    IEnumerable<Vertex> GetVertices(ItemStack stack);
}

public class SimpleBakedItemModel: IBakedItemModel {

    private readonly List<Vertex> _vertices;
    
    public SimpleBakedItemModel(List<Vertex> vertices){
        _vertices = vertices;
    }
    
    public IEnumerable<Vertex> GetVertices(ItemStack stack){
        return _vertices;
    }
}