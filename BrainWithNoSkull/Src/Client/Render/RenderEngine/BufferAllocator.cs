using System.Diagnostics.CodeAnalysis;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public class BufferAllocator {
    public record MemoryBlock(long Offset, long Size);

    private readonly long _totalSize;
    private long _freeMemoryRemaining;
    private readonly List<MemoryBlock> _blocks;

    public BufferAllocator(long totalSize){
        _totalSize = totalSize;
        _freeMemoryRemaining = totalSize;
        _blocks = [new MemoryBlock(0, totalSize)];
    }

    public bool TryAllocate(long size, [MaybeNullWhen(false)] out MemoryBlock outBlock){
        for (int i = 0; i < _blocks.Count; i++) {
            MemoryBlock block = _blocks[i];
            if (size <= block.Size) {
                outBlock = block;
                if (size == block.Size) {
                    _blocks.RemoveAt(i);
                }
                else {
                    _blocks[i] = new MemoryBlock(block.Offset + size, block.Size - size);
                }

                _freeMemoryRemaining -= size;
                return true;
            }
        }

        outBlock = null;
        return false;
    }

    public void FreeBlock(MemoryBlock block){
        _freeMemoryRemaining += block.Size;
        int insertIndex = _blocks.FindIndex(otherBlock => otherBlock.Offset > block.Offset);
        if (insertIndex == -1) {
            _blocks.Add(block);
        }
        else {
            _blocks.Insert(insertIndex, block);

            bool finishedMerge = false;
            while (!finishedMerge) {
                MemoryBlock next = _blocks[insertIndex + 1];
                if (block.Offset + block.Size == next.Offset) {
                    _blocks[insertIndex] = block with{ Size = block.Size + next.Size };
                    _blocks.RemoveAt(insertIndex + 1);
                }
                else {
                    finishedMerge = true;
                }
            }
        }
    }
}