using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.World;

namespace BrainWithNoSkull.Server;

public class ServerWorld : WorldBase {
    
    public ServerWorld(WorldSettings settings) : base(settings, true){
    }
}