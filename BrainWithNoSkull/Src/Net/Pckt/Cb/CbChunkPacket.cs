using BrainWithNoSkull.Blocks;
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
        
    }

    public void Read(PacketReader reader){
        
    }

    public void Handle(PacketContext ctx){
        
    }
}