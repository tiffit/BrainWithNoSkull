using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public interface IChunkGenerator {

    void Generate(Vector2D<int> chunkCoords, int size, GenChunk chunk);

}