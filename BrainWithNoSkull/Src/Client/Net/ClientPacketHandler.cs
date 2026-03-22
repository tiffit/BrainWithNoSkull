using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;

namespace BrainWithNoSkull.Client.Net;

public class ClientPacketHandler : IClientPacketHandler {

    private readonly IServerConnection _serverConnection;
    private readonly GameClient _client;
    
    public ClientPacketHandler(IServerConnection serverConnection, GameClient client){
        _serverConnection = serverConnection;
        _client = client;
    }

    public void HandeWorldInfo(CbWorldInfoPacket packet){
        
    }

    public void HandeChunk(CbChunkPacket packet){
        
    }
}