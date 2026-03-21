using BrainWithNoSkull.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbChunkPacket : IPacket {

    private Block[,,] _blocks;
    private Vector2D<int> _chunkCoords;
    
    public CbChunkPacket(){}
    
    public CbChunkPacket(Block[,,] blocks, Vector2D<int> chunkCoords){
        _blocks = blocks;
        _chunkCoords = chunkCoords;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Chunk;
    }

    public void Write(PacketWriter writer){
        throw new NotImplementedException();
    }

    public void Read(PacketReader reader){
        throw new NotImplementedException();
    }
}