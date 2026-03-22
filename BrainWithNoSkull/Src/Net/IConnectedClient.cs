using BrainWithNoSkull.Net.Pckt;

namespace BrainWithNoSkull.Net;

public interface IConnectedClient {


    public void SendPacket(IPacket packet);

    public void Tick(double delta);
}