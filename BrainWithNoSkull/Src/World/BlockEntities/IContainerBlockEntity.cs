namespace BrainWithNoSkull.World.BlockEntities;

public interface IContainerBlockEntity {

    public IItemContainer GetContainer();
    public IItemContainer GetInputContainer();
    public IItemContainer GetOutputContainer();

}