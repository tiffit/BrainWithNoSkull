using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Server;

public class GameServerBase {

    public ServerWorld World;
    private readonly List<ConnectedClientBase> _clients = [];
    
    public GameServerBase(){
        World = new ServerWorld(new WorldSettings(){ WorldSize = 4 });
    }

    public virtual void Tick(double delta){
        World.Tick(delta);
    }

    public void Connect(ConnectedClientBase client){
        _clients.Add(client);
        client.SendPacket(new CbWorldInfoPacket(World.Settings));
        ServerPlayerEntity player = new ServerPlayerEntity(client, World);
        client.Entity = player;
        World.AddEntity(player, new Vector3D<float>(0.5f, Constants.SeaLevel, 0.5f));
    }
    
}