using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Net;

public abstract class ConnectedClientBase {

    public ServerPlayerEntity? Entity;
    
    public abstract void SendPacket(IPacket packet);

    public abstract void Tick(double delta);
}