using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbChunkPacket : IPacket {

    public Block[,,] Blocks;
    public uint[,,] BlockConfigs;
    public byte[,,] Light;
    public Vector2D<int> ChunkCoords;

    public CbChunkPacket(){
        Blocks = null!;
    }
    
    public CbChunkPacket(Block[,,] blocks, uint[,,] blockConfigs, byte[,,] light, Vector2D<int> chunkCoords){
        Blocks = blocks;
        BlockConfigs = blockConfigs;
        Light = light;
        ChunkCoords = chunkCoords;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Chunk;
    }

    public void Write(PacketWriter writer){
        writer.Write(ChunkCoords);
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    writer.Write(Blocks[x,y,z].GetIndex());
                    writer.Write(BlockConfigs[x,y,z]);
                    writer.Write(Light[x,y,z]);
                }
            }
        }
    }

    public void Read(PacketReader reader){
        ChunkCoords = reader.Read2DIntVec();
        Blocks = new Block[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
        BlockConfigs = new uint[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
        Light = new byte[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    Blocks[x, y, z] = BlockRegistry.Registry.GetEntry(reader.ReadUInt16()) ?? BlockRegistry.Air;
                    BlockConfigs[x, y, z] = reader.ReadUInt32();
                    Light[x, y, z] = reader.ReadByte();
                }
            }
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandeChunk(this);
    }
}