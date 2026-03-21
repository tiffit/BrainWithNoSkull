using System.Diagnostics;
using BrainWithNoSkull.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientWorld : WorldBase {

    private readonly Dictionary<Vector2D<int>, ChunkMesh> _chunkMeshes = new Dictionary<Vector2D<int>, ChunkMesh>();

    private int _generatedChunks = 0;
    
    public ClientWorld(WorldSettings settings) : base(settings, true){
    }

    public void Render(double delta){
        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int x = -16; x < 16; x++) {
            for (int z = -16; z < 16; z++) {
                Vector2D<int> chunkCoords = new Vector2D<int>(x, z);
                RenderChunk(chunkCoords);
            }
        }
        sw.Stop();
        if (_generatedChunks > 0) {
            Console.WriteLine("Took " + sw.ElapsedMilliseconds + " to generate " + _generatedChunks + " chunks");
        }
        _generatedChunks = 0;
    }

    private void RenderChunk(Vector2D<int> chunkCoords){
        Chunk chunk = GetChunk(chunkCoords);
        if (!_chunkMeshes.ContainsKey(chunkCoords)) {
            ChunkMesh mesh = new ChunkMesh(chunkCoords);
            mesh.FullyGenerate(Chunks);
            _chunkMeshes[chunkCoords] = mesh;
            _generatedChunks++;
        }
    }
}