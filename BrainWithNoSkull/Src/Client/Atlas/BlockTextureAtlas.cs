using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class BlockTextureAtlas : TextureAtlas {

    private readonly Dictionary<string, AtlasLocation> _locations = new Dictionary<string, AtlasLocation>();
    
    public void Create(ICollection<string> paths){
        _locations.Clear();
        SKBitmap atlas = new SKBitmap(64, 64, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas atlasCanvas = new SKCanvas(atlas);
        int index = 0;
        Dictionary<string, AtlasLocation> locations = new Dictionary<string, AtlasLocation>();
        
        foreach (string path in paths) {
            string textureLoc = $"./Assets/Textures/{path}.png";
            SKBitmap bitmap = SKBitmap.Decode(textureLoc);
            uint width = (uint)bitmap.Width;
            uint height = (uint)bitmap.Height;
            Span<byte> pixels = bitmap.GetPixelSpan();
            ulong imageSize = (ulong)bitmap.ByteCount;

            int atlasX = index % 8;
            int atlasY = index / 8;
            atlasCanvas.DrawBitmap(bitmap, atlasX*8, atlasY*8);
            locations[textureLoc] = new AtlasLocation(atlasX, atlasY, atlasX + 8, atlasY + 8);
            index++;
        }
        atlasCanvas.Flush();
        
        foreach ((string path, AtlasLocation loc) in locations) {
            _locations[path] = loc / atlas.Width;
        }

        // using FileStream fileStream = File.OpenWrite("./Assets/Textures/BlockAtlas.png");
        // using SKData data = atlas.Encode(SKEncodedImageFormat.Png, 100);
        // data.SaveTo(fileStream);
        // fileStream.Flush();
    }

    public AtlasLocation Lookup(string path){
        return _locations[path];
    }
    
}