using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Server;

public class ServerWorld : WorldBase {
    
    public ServerWorld(WorldSettings settings) : base(settings, true){
    }
}