using BrainWithNoSkull.Net;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientPlayerEntity : PlayerEntity {

    public float BlockBreakTime;
    public Vector3D<int>? BlockBreakPosition;
    public bool BlockBreakPause = false;
    
    public ServerConnectionBase ServerConnection{ get; private set; }
    
    public ClientPlayerEntity(ServerConnectionBase serverConnection, WorldBase? world) : base(world){
        ServerConnection = serverConnection;
    }
    
}