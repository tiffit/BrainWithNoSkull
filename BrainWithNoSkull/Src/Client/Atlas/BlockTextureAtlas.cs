using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class BlockTextureAtlas : TextureAtlas {
    private readonly Dictionary<string, uint> _indices = new Dictionary<string, uint>();

    private const int SpriteSize = 8;
    private const int SpritePadding = 4;
    private const int SpriteSizeWithPadding = SpriteSize + SpritePadding * 2;

    public void Create(ICollection<string> paths){
        Textures.Clear();
        uint index = 0;
        foreach (string path in paths) {
            SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/{path}.png");
            Textures.Add(bitmap);
            _indices[path] = index;
            index++;
        }
    }

    public uint Lookup(string path){
        return _indices[path];
    }
}