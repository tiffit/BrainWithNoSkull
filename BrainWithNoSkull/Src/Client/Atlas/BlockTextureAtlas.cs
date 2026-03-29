using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class BlockTextureAtlas : TextureAtlas {
    private readonly Dictionary<string, uint> _blockIndices = new Dictionary<string, uint>();
    private readonly Dictionary<string, uint> _guiIndices = new Dictionary<string, uint>();

    private uint _nextIndex;
    
    public void Create(ICollection<string> paths){
        foreach (string path in paths) {
            SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/{path}.png");
            Textures.Add(bitmap);
            _blockIndices[path] = _nextIndex++;
        }
    }
    
    public void CreateGuiTextures(){
        string searchDir = Path.GetFullPath("./Assets/Textures/Gui/");
        foreach (string path in Directory.GetFiles(searchDir, "*.png", SearchOption.AllDirectories)) {
            string name = Path.GetFileNameWithoutExtension(path[searchDir.Length..]);
            SKBitmap bitmap = SKBitmap.Decode(path);
            Textures.Add(bitmap);
            _guiIndices[name] = _nextIndex++;
        }
    }

    public uint LookupBlock(string path){
        return _blockIndices[path];
    }
    
    public uint LookupGui(string path){
        return _guiIndices[path];
    }
}