using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbFullLightPacket : IPacket {

    public byte[,,] Light = null!;
    public Vector2D<int> ChunkCoords;

    public CbFullLightPacket(){}
    
    public CbFullLightPacket(byte[,,] light, Vector2D<int> chunkCoords){
        Light = light;
        ChunkCoords = chunkCoords;
    }
    
    public PacketType GetPacketType(){
        return PacketType.FullLight;
    }

    public void Write(PacketWriter writer){
        writer.Write(ChunkCoords);
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    writer.Write(Light[x,y,z]);
                }
            }
        }
    }

    public void Read(PacketReader reader){
        ChunkCoords = reader.Read2DIntVec();
        Light = new byte[Constants.ChunkSize, Constants.MaxWorldHeight, Constants.ChunkSize];
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < Constants.ChunkSize; z++) {
                    Light[x, y, z] = reader.ReadByte();
                }
            }
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleFullLight(this);
    }
}