using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class ConveyorBlock: NonFullBlock, IBlockEntityProvider {
    
    private static readonly AABB CollisionBox = new AABB(1/8f, 0, 1/8f, 7/8f, 4/8f, 7/8f);
    
    public ConveyorBlock(string id) : base(id){
        ConfigContainer.With(HorizontalFacingBlockConfigType.Instance);
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }

    public BlockEntity GetBlockEntity(){
        return BlockEntityRegistry.Conveyor.Create();
    }
    
    public override uint GetPlaceConfig(PlayerEntity player, Vector3D<int> blockPos){
        uint config = base.GetPlaceConfig(player, blockPos);
        config = HorizontalFacingBlockConfigType.SetPlaceDirection(config, ConfigContainer, player, blockPos);
        return config;
    }
}