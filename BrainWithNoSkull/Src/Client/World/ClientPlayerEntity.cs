using BrainWithNoSkull.Net;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.World;

public class ClientPlayerEntity : PlayerEntity {
    
    public IServerConnection ServerConnection{ get; private set; }
    
    public ClientPlayerEntity(IServerConnection serverConnection, WorldBase? world) : base(world){
        ServerConnection = serverConnection;
    }
    
}