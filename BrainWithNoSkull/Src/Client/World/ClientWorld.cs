using System.Diagnostics;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientWorld : WorldBase {
    private readonly Dictionary<Vector2D<int>, ChunkMesh> _chunkMeshes = new Dictionary<Vector2D<int>, ChunkMesh>();
    private readonly GameClient _gameClient;
    private readonly ServerConnectionBase _serverConnection;

    private int ViewDistance = 8;
    private Vector2D<int>? _playerChunk;
    private bool _newChunkLoaded = false;

    public ClientWorld(GameClient client, ServerConnectionBase serverConnection, WorldSettings settings) : base(settings, false){
        _gameClient = client;
        _serverConnection = serverConnection;
    }

    public void Render(double delta){
        ClientPlayerEntity? player = _serverConnection.PlayerEntity;
        if (player?.Chunk != null) {
            // if (_newChunkLoaded || _playerChunk != player.Chunk.Coords) {
                _newChunkLoaded = false;
                _playerChunk = player.Chunk.Coords;

                List<Vector2D<int>> cleared = [];
                foreach ((Vector2D<int> coords, ChunkMesh mesh) in _chunkMeshes) {
                    Vector2D<int> chunkDiff = ChunkUtils.ToChunkCoords(player.Position) - coords;
                    if (Math.Max(Math.Abs(chunkDiff.X), Math.Abs(chunkDiff.Y)) > ViewDistance + 1) {
                        mesh.Unload();
                        cleared.Add(coords);
                    }
                }
                
                foreach (Vector2D<int> toClear in cleared) {
                    _chunkMeshes.Remove(toClear);
                }
                
                for (int x = -ViewDistance; x <= ViewDistance; x++) {
                    for (int z = -ViewDistance; z <= ViewDistance; z++) {
                        Vector2D<int> chunkCoords = new Vector2D<int>(x, z) + ChunkUtils.ToChunkCoords(player.Position);
                        if (Chunks.IsChunkLoaded(chunkCoords)) {
                            TryGenerateMesh(chunkCoords);
                        }
                    }
                }
            // }
        }
    }

    public void AddPlayer(PlayerEntity player, CbPlayerInfoPacket packet){
        AddEntity(player, packet.PlayerPos);
        _playerChunk = null;
    }

    public void OnChunkLoaded(Chunk chunk){
        Chunks.LoadChunk(chunk);
        _newChunkLoaded = true;
    }

    private void TryGenerateMesh(Vector2D<int> chunkCoords){
        if (!_chunkMeshes.ContainsKey(chunkCoords)) {
            ChunkMesh mesh = new ChunkMesh(chunkCoords);
            if (mesh.FullyGenerate(Chunks)) {
                _chunkMeshes[chunkCoords] = mesh;
            }
        }
    }
}