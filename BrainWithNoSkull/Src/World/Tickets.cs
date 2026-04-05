using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public record EntityMoveChunkTicket(Entity Entity, Chunk OldChunk, Vector2D<int> NewChunk);
public record EntityRemoveTicket(uint Id);