using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked.Specific;

public class ConveyorBlockModel: UnbakedBlockModel {

    public static readonly ConveyorBlockModel Instance = new ConveyorBlockModel();
    public uint ConveyorBelt;
    
    public override void GatherDependencies(){
        TextureDependencies = ["Block/Conveyor"];
    }

    public override IBakedBlockModel Bake(TextureResolver resolver, uint packedConfig){
        ConveyorBelt = resolver("Block/Conveyor");
        return new SimpleBakedBlockModel([], new Dictionary<Direction, List<Vertex>>());
    }
}