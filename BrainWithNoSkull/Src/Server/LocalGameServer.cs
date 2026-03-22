using BrainWithNoSkull.Client.Net;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;

namespace BrainWithNoSkull.Server;

public class LocalGameServer : GameServerBase {

    private LocalConnectedClient _client;

    public LocalGameServer() : base(){ }


}