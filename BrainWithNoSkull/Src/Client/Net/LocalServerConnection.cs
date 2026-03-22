using System.Collections.Concurrent;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Server;

namespace BrainWithNoSkull.Client.Net;

public class LocalServerConnection : IServerConnection {
    
    private readonly LocalGameServer _gameServer;
    private readonly ConcurrentQueue<IPacket> _packetQueue = new ConcurrentQueue<IPacket>();
    private LocalConnectedClient _connectedClient = null!;
    private readonly byte[] _buffer = new byte[Constants.NetworkBufferSize];

    public LocalServerConnection(LocalGameServer gameServer){
        _gameServer = gameServer;
    }

    public void Connect(){
        _connectedClient = new LocalConnectedClient(_gameServer, this);
        _gameServer.Connect(_connectedClient);
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
        _connectedClient.ReceivePacket(newPacket);
    }

    public void Tick(double delta){
        while (_packetQueue.TryDequeue(out IPacket? packet)) {
            packet.Handle(new PacketContext(null));
        }
    }
    
}