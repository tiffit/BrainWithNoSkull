using BrainWithNoSkull.Net;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.World;

public class ClientPlayerEntity : PlayerEntity {
    
    public ServerConnectionBase ServerConnection{ get; private set; }
    
    public ClientPlayerEntity(ServerConnectionBase serverConnection, WorldBase? world) : base(world){
        ServerConnection = serverConnection;
    }
    
}