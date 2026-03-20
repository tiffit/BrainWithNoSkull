using BrainWithNoSkull.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientWorld : WorldBase {

    private readonly Dictionary<Vector2D<int>, ChunkMesh> _chunkMeshes = new Dictionary<Vector2D<int>, ChunkMesh>();
    
    public ClientWorld(WorldSettings settings) : base(settings){
    }

    public void Render(double delta){
        for (int x = 0; x < Settings.WorldSize; x++) {
            for (int z = 0; z < Settings.WorldSize; z++) {
                Vector2D<int> chunkCoords = new Vector2D<int>(x, z);
                RenderChunk(chunkCoords);
            }
        }
    }

    private void RenderChunk(Vector2D<int> chunkCoords){
        Chunk chunk = GetChunk(chunkCoords);
        if (!_chunkMeshes.ContainsKey(chunkCoords)) {
            ChunkMesh mesh = new ChunkMesh(chunkCoords);
            mesh.FullyGenerate(Chunks);
            _chunkMeshes[chunkCoords] = mesh;
        }
    }
}