using BrainWithNoSkull.Net.Pckt.Cb;

namespace BrainWithNoSkull.Net;

public interface IClientPacketHandler {

    public void HandeWorldInfo(CbWorldInfoPacket packet);
    public void HandeChunk(CbChunkPacket packet);
    public void HandlePlayerInfo(CbPlayerInfoPacket packet);
    public void HandlePlayerInventory(CbPlayerInventoryPacket packet);
    public void HandleNewEntity(CbNewEntityPacket packet);
    public void HandleEntityMove(CbEntityMovePacket packet);
    public void HandleRemoveEntity(CbRemoveEntityPacket packet);
    public void HandleNewBlockEntity(CbNewBlockEntityPacket packet);
    public void HandleOpenContainer(CbOpenContainerPacket packet);
}