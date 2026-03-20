using System.Diagnostics;
using BrainWithNoSkull.Blocks;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Util;
using BrainWithNoSkull.World;
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
        Chunk chunk = storage.GetChunk(_chunkCoords);
        for (int i = 0; i < _sections.Length; i++) {
            if (_sections[i]?.Mesh != null) {
                GameClient.Instance.RenderEngine.DestroyMesh(_sections[i]!.Mesh!);
            }
            _sections[i] = GenerateSection(storage, chunk, i);
        }
    }

    private ChunkMeshSection GenerateSection(ChunkStorage storage, Chunk chunk, int sectionIndex){
        IRenderEngine renderEngine = GameClient.Instance.RenderEngine;
        VertexBuilder builder = new VertexBuilder(renderEngine, ShaderType.Shader, new MatrixStack());
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.ChunkSize; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    Vector3D<int> blockPos = new Vector3D<int>(x, y + sectionIndex * Constants.ChunkSize, z);
                    Vector3D<float> blockPosFlt = (Vector3D<float>)blockPos;
                    Block block = chunk.GetBlock(blockPos.X, blockPos.Y, blockPos.Z);
                    if (!block.IsAir()) {
                        if (chunk.GetBlock(blockPos.X, blockPos.Y, blockPos.Z + 1).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt + Vector3D<float>.UnitZ,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitZ,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitX + Vector3D<float>.UnitZ,
                                blockPosFlt + Vector3D<float>.UnitX + Vector3D<float>.UnitZ,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(0, 1),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(1, 0),
                                transformPos: false
                            );
                        }

                        if (chunk.GetBlock(blockPos.X, blockPos.Y, blockPos.Z - 1).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt,
                                blockPosFlt + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitY,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1),
                                transformPos: false
                            );
                        }

                        if (chunk.GetBlock(blockPos.X, blockPos.Y - 1, blockPos.Z).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt,
                                blockPosFlt + Vector3D<float>.UnitZ,
                                blockPosFlt + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitX,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1),
                                transformPos: false
                            );
                        }

                        if (chunk.GetBlock(blockPos.X, blockPos.Y + 1, blockPos.Z).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt + Vector3D<float>.UnitY,
                                blockPosFlt + Vector3D<float>.UnitX + Vector3D<float>.UnitY,
                                blockPosFlt + Vector3D<float>.UnitZ + Vector3D<float>.UnitX + Vector3D<float>.UnitY,
                                blockPosFlt + Vector3D<float>.UnitZ + Vector3D<float>.UnitY,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(0, 1),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(1, 0),
                                transformPos: false
                            );
                        }

                        if (chunk.GetBlock(blockPos.X + 1, blockPos.Y, blockPos.Z).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitZ + Vector3D<float>.UnitX,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitX,
                                new Vector2D<float>(0, 0),
                                new Vector2D<float>(1, 0),
                                new Vector2D<float>(1, 1),
                                new Vector2D<float>(0, 1),
                                transformPos: false
                            );
                        }

                        if (chunk.GetBlock(blockPos.X - 1, blockPos.Y, blockPos.Z).IsAir()) {
                            VertexHelper.AddQuad(builder,
                                blockPosFlt,
                                blockPosFlt + Vector3D<float>.UnitY,
                                blockPosFlt + Vector3D<float>.UnitY + Vector3D<float>.UnitZ,
                                blockPosFlt + Vector3D<float>.UnitZ,
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

    private class ChunkMeshSection {
        public Mesh? Mesh;
    }
}