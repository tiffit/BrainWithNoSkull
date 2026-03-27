using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class BlockTextureAtlas : TextureAtlas {
    private readonly Dictionary<string, AtlasLocation> _locations = new Dictionary<string, AtlasLocation>();

    private const int SpriteSize = 8;
    private const int SpritePadding = 4;
    private const int SpriteSizeWithPadding = SpriteSize + SpritePadding * 2;

    public void Create(ICollection<string> paths){
        _locations.Clear();
        AtlasTexture = new SKBitmap(128, 128, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas atlasCanvas = new SKCanvas(AtlasTexture);
        int index = 0;
        Dictionary<string, AtlasLocation> locations = new Dictionary<string, AtlasLocation>();

        int tilesPerRow = AtlasTexture.Width / SpriteSizeWithPadding;
        foreach (string path in paths) {
            SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/{path}.png");

            int atlasX = (index % tilesPerRow) * SpriteSizeWithPadding;
            int atlasY = (index / tilesPerRow) * SpriteSizeWithPadding;

            for (int x = 0; x < SpriteSizeWithPadding; x++) {
                for (int y = 0; y < SpriteSizeWithPadding; y++) {
                    int sampleX = int.Clamp(x - SpritePadding, 0, SpriteSize - 1);
                    int sampleY = int.Clamp(y - SpritePadding, 0, SpriteSize - 1);
                    SKColor color = bitmap.GetPixel(sampleX, sampleY);
                    atlasCanvas.DrawPoint(atlasX + x, atlasY + y, color);
                }
            }

            locations[path] = new AtlasLocation(atlasX + SpritePadding, atlasY + SpritePadding, atlasX + SpritePadding + SpriteSize, atlasY + SpritePadding + SpriteSize);
            index++;
        }

        atlasCanvas.Flush();

        foreach ((string path, AtlasLocation loc) in locations) {
            _locations[path] = loc / AtlasTexture.Width;
        }

        using FileStream fileStream = File.OpenWrite("./Assets/Textures/BlockAtlas.png");
        using SKData data = AtlasTexture.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(fileStream);
        fileStream.Flush();
    }

    public AtlasLocation Lookup(string path){
        return _locations[path];
    }
}