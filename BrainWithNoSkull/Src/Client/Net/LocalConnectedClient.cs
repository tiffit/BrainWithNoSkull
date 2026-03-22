using System.Collections.Concurrent;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Server;

namespace BrainWithNoSkull.Client.Net;

public class LocalConnectedClient : IConnectedClient {

    private readonly LocalGameServer _gameServer;
    private readonly ConcurrentQueue<IPacket> _packetQueue = new ConcurrentQueue<IPacket>();
    private readonly LocalServerConnection _serverConnection;

    private readonly byte[] _buffer = new byte[Constants.NetworkBufferSize];

    public LocalConnectedClient(LocalGameServer gameServer, LocalServerConnection serverConnection){
        _gameServer = gameServer;
        _serverConnection = serverConnection;
    }

    public void ReceivePacket(IPacket packet){
        _packetQueue.Enqueue(packet);
    }

    public void SendPacket(IPacket packet){
        using MemoryStream ms = new MemoryStream(_buffer);
        using PacketWriter writer = new PacketWriter(ms);
        packet.EncodePacket(writer);
        ms.Position = 0;
        using PacketReader reader = new PacketReader(ms);
        IPacket newPacket = PacketRegistry.DecodePacket(reader);
        _serverConnection.ReceivePacket(newPacket);
    }

    public void Tick(double delta){
        while (_packetQueue.TryDequeue(out IPacket? packet)) {
            packet.Handle(new PacketContext(this, null, null));
        }
    }
    
}