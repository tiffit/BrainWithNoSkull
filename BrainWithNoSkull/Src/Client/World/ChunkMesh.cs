using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Util;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.World;

public class ChunkMesh {
    private readonly Vector2D<int> _chunkCoords;
    private ChunkMeshSection?[] _sections;

    public ChunkMesh(Vector2D<int> chunkCoords){
        _chunkCoords = chunkCoords;
        _sections = new ChunkMeshSection[Constants.MaxWorldHeight / Constants.ChunkSize];
    }

    public void FullyGenerate(ChunkStorage storage){
        Chunk? chunk = storage.GetChunk(_chunkCoords);
        if (chunk != null) {
            for (int i = 0; i < _sections.Length; i++) {
                if (_sections[i]?.Mesh != null) {
                    GameClient.Instance.RenderEngine.DestroyMesh(_sections[i]!.Mesh!);
                }
                _sections[i] = GenerateSection(storage, chunk, i);
            }
        }
    }

    private ChunkMeshSection GenerateSection(ChunkStorage storage, Chunk chunk, int sectionIndex){
        IRenderEngine renderEngine = GameClient.Instance.RenderEngine;
        VertexBuilder builder = new VertexBuilder(renderEngine, ShaderType.Shader, new MatrixStack());
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
                                new Vector2D<float>(1, 0),
                                transformPos: false
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
                                new Vector2D<float>(0, 1),
                                transformPos: false
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
                                new Vector2D<float>(0, 1),
                                transformPos: false
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
                                new Vector2D<float>(1, 0),
                                transformPos: false
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
                                new Vector2D<float>(0, 1),
                                transformPos: false
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
                                new Vector2D<float>(1, 0),
                                transformPos: false
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
            renderEngine.WriteMesh(mesh, builder, ref pushConstant);
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