using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class LightEngine {

    public void FullRebuild(ChunkStorage storage, Chunk chunk){
        chunk.HasPopulatedLighting = true;
        byte[,,] lightArray = chunk.GetPackedLight();

        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int z = 0; z < Constants.ChunkSize; z++) {
                for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                    lightArray[x, y, z] = 0;
                }
            }
        }

        Queue<Vector3D<int>> queue = new Queue<Vector3D<int>>(Constants.ChunkSize * Constants.ChunkSize * (Constants.MaxWorldHeight/2));
        // Skylight downward cast
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int z = 0; z < Constants.ChunkSize; z++) {
                for (int y = Constants.MaxWorldHeight - 1; y >= 0; y--) {
                    Block block = chunk.GetBlock(x, y, z);
                    if (block.IsFullBlock()) {
                        lightArray[x, y, z] = 0;
                        break;
                    }
                    lightArray[x, y, z] = 15 << 4;
                    queue.Enqueue(new Vector3D<int>(x, y, z));
                }
            }
        }

        RebuildLighting(queue, storage, chunk);
    }

    public void RebuildLighting(Queue<Vector3D<int>> queue, ChunkStorage storage, Chunk chunk){
        byte[,,] lightArray = chunk.GetPackedLight();
        while (queue.TryDequeue(out Vector3D<int> result)) {
            byte lightLevel = (byte)((lightArray[result[0], result[1], result[2]] >> 4) - 1);
            AddToQueue(queue, storage, chunk, result[0] - 1, result[1], result[2], lightArray, lightLevel);
            AddToQueue(queue, storage, chunk, result[0], result[1] - 1, result[2], lightArray, lightLevel);
            AddToQueue(queue, storage, chunk, result[0], result[1], result[2] - 1, lightArray, lightLevel);
            
            AddToQueue(queue, storage, chunk, result[0] + 1, result[1], result[2], lightArray, lightLevel);
            AddToQueue(queue, storage, chunk, result[0], result[1] + 1, result[2], lightArray, lightLevel);
            AddToQueue(queue, storage, chunk, result[0], result[1], result[2] + 1, lightArray, lightLevel);
        }
    }

    private void AddToQueue(Queue<Vector3D<int>> queue, ChunkStorage storage, Chunk chunk, int x, int y, int z, byte[,,] lightArray, byte lightLevel){
        if (x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize) {
            return;
        }

        if (lightArray[x, y, z] >> 4 >= lightLevel) {
            return;
        }
        
        Block block = chunk.GetBlock(x, y, z);
        if (block.IsFullBlock()) {
            return;
        }

        lightArray[x, y, z] = (byte)(lightLevel << 4);
        if (lightLevel > 1) {
            queue.Enqueue(new Vector3D<int>(x, y, z));
        }
    }
}