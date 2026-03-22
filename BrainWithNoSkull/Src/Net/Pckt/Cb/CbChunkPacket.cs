using BrainWithNoSkull.Blocks;
using BrainWithNoSkull.Registries;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbChunkPacket : IPacket {

    private Block[,,] _blocks;
    private Vector2D<int> _chunkCoords;

    public CbChunkPacket(){
        _blocks = null!;
    }
    
    public CbChunkPacket(Block[,,] blocks, Vector2D<int> chunkCoords){
        _blocks = blocks;
        _chunkCoords = chunkCoords;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Chunk;
    }

    public void Write(PacketWriter writer){
        writer.Write(_chunkCoords);
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    writer.Write(_blocks[x,y,z].GetIndex());
                }
            }
        }
    }

    public void Read(PacketReader reader){
        _chunkCoords = reader.Read2DIntVec();
        _blocks = new Block[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    _blocks[x, y, z] = BlockRegistry.Registry.GetEntry(reader.ReadUInt16()) ?? BlockRegistry.Air;
                }
            }
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandeChunk(this);
    }
}