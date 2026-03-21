using BrainWithNoSkull.Client.World;

namespace BrainWithNoSkull.Server;

public class GameServerBase {

    private ServerWorld _world;
    
    public GameServerBase(){
        _world = new ServerWorld(new WorldSettings(){ WorldSize = 4 });
    }

    public virtual void Tick(double delta){
        _world.Tick(delta);
    }
    
}