using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Commands.Components;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Server;

public class GameServerBase {

    public ServerWorld World;
    private readonly CommandRegistry _commandRegistry;
    private readonly List<ConnectedClientBase> _clients = [];
    
    public GameServerBase(){
        World = new ServerWorld(new WorldSettings{ WorldSize = 512 });
        _commandRegistry = new CommandRegistry(this);
        _commandRegistry.Init();
    }

    public virtual void Tick(double delta){
        foreach (ConnectedClientBase client in _clients) {
            client.Tick(delta);
        }

        World.Tick(delta);
    }

    public void Connect(ConnectedClientBase client){
        client.Server = this;
        _clients.Add(client);
        client.SendPacket(new CbWorldInfoPacket(World.Settings));
        ServerPlayerEntity player = new ServerPlayerEntity(client, World);
        client.Entity = player;
        float center = (World.Settings.WorldSize * 16) / 2;
        World.AddEntity(player, new Vector3D<float>(center/2, Constants.SeaLevel + 5, center));
    }

    public void ExecuteCommand(ICommandExecutor executor, string text){
        Console.WriteLine("Got command /" + text);
        _commandRegistry.Execute(executor, new CommandInput(text));
    }
}