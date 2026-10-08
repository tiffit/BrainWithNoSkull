using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;

namespace BrainWithNoSkull.World.Entities.Player;

public class ServerPlayerEntity : PlayerEntity {
    
    public ConnectedClientBase Client{ get; private set; }
    
    public ServerPlayerEntity(ConnectedClientBase client, WorldBase? world) : base(world){
        Client = client;
    }

    public void UpdateFlags(){
        Client.SendPacket(new CbPlayerFlagsPacket(this));
    }
}