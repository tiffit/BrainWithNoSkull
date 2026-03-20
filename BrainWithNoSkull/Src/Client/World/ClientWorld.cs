using System.Diagnostics;
using BrainWithNoSkull.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ClientWorld : WorldBase {

    private readonly Dictionary<Vector2D<int>, ChunkMesh> _chunkMeshes = new Dictionary<Vector2D<int>, ChunkMesh>();

    private int _generatedChunks = 0;
    
    public ClientWorld(WorldSettings settings) : base(settings){
    }

    public void Render(double delta){
        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int x = -Settings.WorldSize + 1; x < Settings.WorldSize; x++) {
            for (int z = -Settings.WorldSize + 1; z < Settings.WorldSize; z++) {
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