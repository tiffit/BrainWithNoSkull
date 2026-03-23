using BrainWithNoSkull.Net;

namespace BrainWithNoSkull.World.Entities.Player;

public class ServerPlayerEntity : PlayerEntity {
    
    public ConnectedClientBase Client{ get; private set; }
    
    public ServerPlayerEntity(ConnectedClientBase client, WorldBase? world) : base(world){
        Client = client;
    }

    public override void Tick(){
        base.Tick();
        Console.WriteLine("Ticking player");
    }
}