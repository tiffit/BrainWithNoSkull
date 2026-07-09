using System.Diagnostics;
using BrainWithNoSkull.Client;

namespace BrainWithNoSkull.Server;

public class LocalGameServer : GameServerBase {

    private readonly GameClient _client;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private Thread _thread = null!;
    private long _gameTotalTime = 0;

    public LocalGameServer(GameClient client) : base(){
        _client = client;
    }

    public void StartTicking(){
        _stopwatch.Reset();
        _stopwatch.Start();
        _gameTotalTime = 0;
        _thread = new Thread(Run){
            Name = "Server Thread",
            IsBackground = false
        };
        _thread.Start();
    }

    private void Run(){
        long tickTime = 1000 / Constants.TicksPerSecond;
        while (!GameClient.Instance.GameWindow.IsClosing) {
            _gameTotalTime += tickTime;
            Tick(tickTime / 1000f);
            long diff = _gameTotalTime - _stopwatch.ElapsedMilliseconds;
            if (diff > 1) {
                Thread.Sleep((int)diff);
            }
        }
    }


}