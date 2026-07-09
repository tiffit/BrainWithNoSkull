using BrainWithNoSkull.Client.Models.Blocks;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Client.Render.Shader.PushConstants;
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

    public void RebuildSection(ChunkStorage storage, int sectionIndex){
        if (sectionIndex < 0 || sectionIndex >= _sections.Length)
            return;
        if (_sections[sectionIndex]?.Mesh != null) {
            GameClient.Instance.RenderEngine.DestroyMesh(_sections[sectionIndex]!.Mesh!);
        }
        Chunk? chunk = storage.GetChunk(_chunkCoords);
        if (chunk != null) {
            _sections[sectionIndex] = GenerateSection(storage, chunk, sectionIndex);
        }
    }

    private bool CanGenerate(ChunkStorage storage){
        return storage.AdjacentGenerated(_wrappedChunkCoords);
    }

    private ChunkMeshSection GenerateSection(ChunkStorage storage, Chunk chunk, int sectionIndex){
        IRenderEngine renderEngine = GameClient.Instance.RenderEngine;
        VertexBuilder builder = new VertexBuilder(renderEngine, ShaderType.Shader);
        byte[,,] packedLight = chunk.GetPackedLight();
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.ChunkSize; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    int blockX = x;
                    int blockY = y + sectionIndex * Constants.ChunkSize;
                    int blockZ = z;
                    Vector3D<float> blockPos = new Vector3D<float>(blockX, blockY, blockZ);
                    Block block = chunk.GetBlock(blockX, blockY, blockZ);
                    if (!block.IsAir() && BlockModels.BakedModels.TryGetValue(block, out IBakedBlockModel? model)) {
                        List<ValueTuple<Direction, uint>> visible = new List<ValueTuple<Direction, uint>>(6);

                        uint light = 0;
                        if(!GetBlock(chunk, storage, blockX, blockY, blockZ + 1, ref light).ShouldCullFace(Direction.West))  visible.Add(ValueTuple.Create(Direction.East, light));
                        if(!GetBlock(chunk, storage, blockX, blockY, blockZ - 1, ref light).ShouldCullFace(Direction.East))  visible.Add(ValueTuple.Create(Direction.West, light));
                        if(!GetBlock(chunk, storage, blockX, blockY + 1, blockZ, ref light).ShouldCullFace(Direction.Down))  visible.Add(ValueTuple.Create(Direction.Up, light));
                        if(!GetBlock(chunk, storage, blockX, blockY - 1, blockZ, ref light).ShouldCullFace(Direction.Up))    visible.Add(ValueTuple.Create(Direction.Down, light));
                        if(!GetBlock(chunk, storage, blockX + 1, blockY, blockZ, ref light).ShouldCullFace(Direction.South)) visible.Add(ValueTuple.Create(Direction.North, light));
                        if(!GetBlock(chunk, storage, blockX - 1, blockY, blockZ, ref light).ShouldCullFace(Direction.North)) visible.Add(ValueTuple.Create(Direction.South, light));
                        
                        builder.AddAll(model.GetVertices(blockPos, block, chunk.GetBlockConfig(blockX, blockY, blockZ), packedLight[blockX, blockY, blockZ], visible));
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

    private Block GetBlock(Chunk chunk, ChunkStorage storage, int x, int y, int z, ref uint light){
        storage.GetChunkRelativeBlock(ref chunk, ref x, ref y, ref z);
        if (y is < 0 or >= Constants.MaxWorldHeight) {
            light = 15 << 4;
            return BlockRegistry.Air;
        }

        light = chunk.GetPackedLight()[x, y, z];
        return chunk.GetBlock(x, y, z);
    }

    private class ChunkMeshSection {
        public Mesh? Mesh;
    }
}