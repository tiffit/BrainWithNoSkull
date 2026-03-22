using BrainWithNoSkull.Net.Pckt;

namespace BrainWithNoSkull.Net;

public interface IServerConnection {
    public void SendPacket(IPacket packet);

    public void Tick(double delta);
}