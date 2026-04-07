using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net.Pckt;

namespace BrainWithNoSkull.Net;

public abstract class ServerConnectionBase {

    public ClientPlayerEntity? PlayerEntity;
    public ContainerSync? Container;
    
    public abstract void SendPacket(IPacket packet);

    public abstract void Tick(double delta);
}