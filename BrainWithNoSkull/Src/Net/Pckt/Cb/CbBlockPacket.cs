using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbBlockPacket : IPacket {

    public Vector3D<int> Position;
    public Block Block = null!;
    public uint Config;
    
    public CbBlockPacket(){}
    
    public CbBlockPacket(Vector3D<int> pos, Block block, uint config){
        Position = pos;
        Block = block;
        Config = config;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Block;
    }

    public void Write(PacketWriter writer){
        writer.Write(Position);
        writer.Write(Block.GetIndex());
        writer.Write(Config);
    }

    public void Read(PacketReader reader){
        Position = reader.Read3DIntVec();
        Block = BlockRegistry.Registry.GetEntry(reader.ReadUInt16())!;
        Config = reader.ReadUInt32();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleBlock(this);
    }
}