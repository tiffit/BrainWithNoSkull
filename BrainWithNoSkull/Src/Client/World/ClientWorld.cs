using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.Entities;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Net.Pckt.Sb;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientWorld : WorldBase {
    private readonly Dictionary<Vector2D<int>, ChunkMesh> _chunkMeshes = new Dictionary<Vector2D<int>, ChunkMesh>();
    private readonly GameClient _gameClient;
    private readonly ServerConnectionBase _serverConnection;

    private int ViewDistance = 8;
    private Vector2D<int>? _playerChunk;
    private bool _newChunkLoaded = false;

    private readonly PriorityQueue<Vector2D<int>, int> _chunkMeshQueue = new PriorityQueue<Vector2D<int>, int>();
    private readonly HashSet<Vector3D<int>> _chunkMeshRebuilds = [];

    public ClientWorld(GameClient client, ServerConnectionBase serverConnection, WorldSettings settings) : base(settings, false){
        _gameClient = client;
        _serverConnection = serverConnection;
    }

    public void Render(double delta, Camera camera){
        ClientPlayerEntity? player = _serverConnection.PlayerEntity;
        if (player?.Chunk != null) {
            if (_newChunkLoaded || _playerChunk != player.Chunk.Coords) {
                _newChunkLoaded = false;
                _chunkMeshQueue.Clear();
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
                            _chunkMeshQueue.Enqueue(chunkCoords, (chunkCoords - _playerChunk.Value).LengthSquared);
                        }
                    }
                }
            }
            
            foreach (Vector3D<int> rebuildCoords in _chunkMeshRebuilds) {
                Vector2D<int> chunkCoords = new Vector2D<int>(rebuildCoords.X, rebuildCoords.Z);
                if (_chunkMeshes.TryGetValue(chunkCoords, out var mesh)) {
                    mesh.RebuildSection(Chunks, rebuildCoords.Y);
                }
            }
            _chunkMeshRebuilds.Clear();
            
            if (_chunkMeshQueue.TryDequeue(out Vector2D<int> generateCoords, out int _)) {
                TryGenerateMesh(generateCoords);
            }
            
            //Entity rendering
            foreach (Vector2D<int> chunkCoords in _chunkMeshes.Keys) {
                Chunk? chunk = Chunks.GetChunk(chunkCoords);
                if (chunk != null) {
                    VertexBuilder vb = GameClient.Instance.RenderEngine.GetVertexBuilder(ShaderType.WorldEntity);
                    foreach (Entity entity in chunk.GetEntities()) {
                        IEntityRenderer? renderer = EntityRendererRegistry.GetRenderer<Entity>(entity.Type);
                        if (renderer != null && (camera.Frustum?.IsBoxInside(entity.GetBoundingBox()) ?? false)) {
                            MatrixStack ms = new MatrixStack();
                            renderer.Render(entity, ms, vb);
                        }
                    }
                }
            }
        }
    }

    public override void Tick(double delta){
        base.Tick(delta);
        if (_serverConnection.PlayerEntity != null) {
            _serverConnection.SendPacket(new SbPlayerMovePacket(_serverConnection.PlayerEntity!.Position));
        }
    }

    public void AddPlayer(ClientPlayerEntity player, CbPlayerInfoPacket packet){
        AddEntity(player, packet.PlayerPos);
        _playerChunk = null;
    }

    public void OnChunkLoaded(Chunk chunk){
        Chunks.LoadChunk(chunk);
        _newChunkLoaded = true;
    }

    private bool TryGenerateMesh(Vector2D<int> chunkCoords){
        if (!_chunkMeshes.ContainsKey(chunkCoords)) {
            ChunkMesh mesh = new ChunkMesh(chunkCoords);
            if (mesh.FullyGenerate(Chunks)) {
                _chunkMeshes[chunkCoords] = mesh;
                return true;
            }
        }

        return false;
    }

    public override void SetBlock(Vector3D<int> blockPos, Block block){
        base.SetBlock(blockPos, block);
        Vector3D<int> sectionCoords = ChunkUtils.ToChunkSectionedCoords(blockPos);
        _chunkMeshRebuilds.Add(sectionCoords);
        int offsetX = blockPos.X - sectionCoords.X * Constants.ChunkSize;
        int offsetY = blockPos.Y - sectionCoords.Y * Constants.ChunkSize;
        int offsetZ = blockPos.Z - sectionCoords.Z * Constants.ChunkSize;
        if (offsetY == 0)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(0, -1, 0));
        else if (offsetY == Constants.ChunkSize - 1)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(0, 1, 0));
        
        if (offsetX == 0)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(-1, 0, 0));
        else if (offsetX == Constants.ChunkSize - 1)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(1, 0, 0));
        
        if (offsetZ == 0)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(0, 0, -1));
        else if (offsetZ == Constants.ChunkSize - 1)_chunkMeshRebuilds.Add(sectionCoords + new Vector3D<int>(0, 0, 1));
    }
}