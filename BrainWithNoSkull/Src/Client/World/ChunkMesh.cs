using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Util;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ChunkMesh {
    private Vector2D<int> _chunkCoords;
    private Vector2D<int> _wrappedChunkCoords;
    private ChunkMeshSection?[] _sections;

    public ChunkMesh(Vector2D<int> chunkCoords){
        _chunkCoords = chunkCoords;
        _sections = new ChunkMeshSection[Constants.MaxWorldHeight / Constants.ChunkSize];
    }

    public void Unload(){
        foreach (var section in _sections) {
            if (section?.Mesh != null) {
                GameClient.Instance.RenderEngine.DestroyMesh(section.Mesh!);
            }
        }
    }

    public bool FullyGenerate(ChunkStorage storage){
        Chunk? chunk = storage.GetChunk(_chunkCoords);
        if (chunk != null) {
            _wrappedChunkCoords = chunk.Coords;
        }
        if (chunk != null && CanGenerate(storage)) {
            for (int i = 0; i < _sections.Length; i++) {
                if (_sections[i]?.Mesh != null) {
                    GameClient.Instance.RenderEngine.DestroyMesh(_sections[i]!.Mesh!);
                }
                _sections[i] = GenerateSection(storage, chunk, i);
            }
            return true;
        }

        return false;
    }

    private bool CanGenerate(ChunkStorage storage){
        return storage.IsChunkLoaded(_wrappedChunkCoords + Vector2D<int>.UnitX) &&
               storage.IsChunkLoaded(_wrappedChunkCoords - Vector2D<int>.UnitX) &&
               storage.IsChunkLoaded(_wrappedChunkCoords + Vector2D<int>.UnitY) &&
               storage.IsChunkLoaded(_wrappedChunkCoords - Vector2D<int>.UnitY);
    }

    private ChunkMeshSection GenerateSection(ChunkStorage storage, Chunk chunk, int sectionIndex){
        IRenderEngine renderEngine = GameClient.Instance.RenderEngine;
        VertexBuilder builder = new VertexBuilder(renderEngine, ShaderType.Shader);
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.ChunkSize; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    int blockX = x;
                    int blockY = y + sectionIndex * Constants.ChunkSize;
                    int blockZ = z;
                    Vector3D<float> blockPos = new Vector3D<float>(blockX, blockY, blockZ);
                    Block block = chunk.GetBlock(blockX, blockY, blockZ);
                    if (!block.IsAir()) {
                        if (GetBlock(chunk, storage, blockX, blockY, blockZ + 1).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos + Vector3D<float>.UnitZ,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitZ,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitX + Vector3D<float>.UnitZ,
                                blockPos + Vector3D<float>.UnitX + Vector3D<float>.UnitZ,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(0, 1),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(1, 0)
                            );
                        }
                    
                        if (GetBlock(chunk, storage, blockX, blockY, blockZ - 1).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos,
                                blockPos + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitY,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1)
                            );
                        }
                    
                        if (GetBlock(chunk, storage, blockX, blockY - 1, blockZ).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos,
                                blockPos + Vector3D<float>.UnitZ,
                                blockPos + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitX,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1)
                            );
                        }
                    
                        if (GetBlock(chunk, storage, blockX, blockY + 1, blockZ).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos + Vector3D<float>.UnitY,
                                blockPos + Vector3D<float>.UnitX + Vector3D<float>.UnitY,
                                blockPos + Vector3D<float>.UnitZ + Vector3D<float>.UnitX + Vector3D<float>.UnitY,
                                blockPos + Vector3D<float>.UnitZ + Vector3D<float>.UnitY,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(0, 1),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(1, 0)
                            );
                        }
                    
                        if (GetBlock(chunk, storage, blockX + 1, blockY, blockZ).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitX,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1)
                            );
                        }
                    
                        if (GetBlock(chunk, storage, blockX - 1, blockY, blockZ).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPos,
                                blockPos + Vector3D<float>.UnitY,
                                blockPos + Vector3D<float>.UnitY + Vector3D<float>.UnitZ,
                                blockPos + Vector3D<float>.UnitZ,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(0, 1),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(1, 0)
                            );
                        }
                    }
                }
            }
        }

        if (builder.VerticesCount > 0) {
            Mesh mesh = renderEngine.CreateMesh(ShaderType.Shader, builder.GetMeshSize());
            TransformPushConstant pushConstant = new TransformPushConstant{
                Transform = Matrix4X4.CreateTranslation(Constants.ChunkSize * _chunkCoords.X, 0f, Constants.ChunkSize * _chunkCoords.Y)
            };
            Vector3D<float> aabbMin = new Vector3D<float>(_chunkCoords.X, sectionIndex, _chunkCoords.Y) * Constants.ChunkSize;
            AABB aabb = new AABB(aabbMin, aabbMin + new Vector3D<float>(Constants.ChunkSize));
            renderEngine.WriteMesh(mesh, builder, aabb, ref pushConstant);
            return new ChunkMeshSection{
                Mesh = mesh
            };
        }
        return new ChunkMeshSection();
    }

    private Block GetBlock(Chunk chunk, ChunkStorage storage, int x, int y, int z){
        if (x < 0) {
            chunk = storage.GetChunk(chunk.Coords - Vector2D<int>.UnitX);
            x += Constants.ChunkSize;
        }
        if (x >= Constants.ChunkSize) {
            chunk = storage.GetChunk(chunk.Coords + Vector2D<int>.UnitX);
            x -= Constants.ChunkSize;
        }
        if (z < 0) {
            chunk = storage.GetChunk(chunk.Coords - Vector2D<int>.UnitY);
            z += Constants.ChunkSize;
        }
        if (z >= Constants.ChunkSize) {
            chunk = storage.GetChunk(chunk.Coords + Vector2D<int>.UnitY);
            z -= Constants.ChunkSize;
        }
        
        if (y < 0 || y >= Constants.MaxWorldHeight) {
            return BlockRegistry.Air;
        }
        
        return chunk.GetBlock(x, y, z);
    }

    private class ChunkMeshSection {
        public Mesh? Mesh;
    }
}