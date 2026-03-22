using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;

namespace BrainWithNoSkull.Server;

public class GameServerBase {

    public ServerWorld World;
    private readonly List<IConnectedClient> _clients = [];
    
    public GameServerBase(){
        World = new ServerWorld(new WorldSettings(){ WorldSize = 4 });
    }

    public virtual void Tick(double delta){
        World.Tick(delta);
    }

    public void Connect(IConnectedClient client){
        _clients.Add(client);
        client.SendPacket(new CbWorldInfoPacket(World.Settings));
    }
    
}