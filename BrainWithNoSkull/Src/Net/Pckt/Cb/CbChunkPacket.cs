using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbChunkPacket : IPacket {

    public bool AlreadyKnown;
    public Block[,,] Blocks;
    public Vector2D<int> ChunkCoords;

    public CbChunkPacket(){
        Blocks = null!;
    }
    
    public CbChunkPacket(Block[,,] blocks, Vector2D<int> chunkCoords){
        Blocks = blocks;
        ChunkCoords = chunkCoords;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Chunk;
    }

    public void Write(PacketWriter writer){
        writer.Write(AlreadyKnown);
        writer.Write(ChunkCoords);
        if (!AlreadyKnown) {
            for (int x = 0; x < Constants.ChunkSize; x++) {
                for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                    for (int z = 0; z < Constants.ChunkSize; z++) {
                        writer.Write(Blocks[x,y,z].GetIndex());
                    }
                }
            }
        }
    }

    public void Read(PacketReader reader){
        AlreadyKnown = reader.ReadBoolean();
        ChunkCoords = reader.Read2DIntVec();
        if (!AlreadyKnown) {
            Blocks = new Block[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
            for (int x = 0; x < Constants.ChunkSize; x++) {
                for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                    for (int z = 0; z < Constants.ChunkSize; z++) {
                        Blocks[x, y, z] = BlockRegistry.Registry.GetEntry(reader.ReadUInt16()) ?? BlockRegistry.Air;
                    }
                }
            }
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandeChunk(this);
    }
}