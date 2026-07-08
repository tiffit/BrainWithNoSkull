namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbChatPacket : IPacket {

    public string Message = null!;
    
    public SbChatPacket(){}
    
    public SbChatPacket(string message){
        Message = message;
    }
    
    public PacketType GetPacketType(){
        return PacketType.Chat;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(Message);
    }

    public void Read(PacketReader reader){
        Message = reader.ReadString();
    }

    public void Handle(PacketContext ctx){
        if (Message.StartsWith('/')) {
            ctx.Client!.Server.ExecuteCommand(ctx.Client!.Entity!, Message[1..]);
        }
    }
}