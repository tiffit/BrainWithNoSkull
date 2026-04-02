using BrainWithNoSkull.Client.Net;

namespace BrainWithNoSkull.Server;

public class LocalGameServer : GameServerBase {

    private LocalConnectedClient _client;

    public LocalGameServer() : base(){ }


}