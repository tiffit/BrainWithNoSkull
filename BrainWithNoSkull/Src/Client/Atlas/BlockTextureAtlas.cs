using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class BlockTextureAtlas : TextureAtlas {
    private readonly Dictionary<string, uint> _blockIndices = new Dictionary<string, uint>();
    private readonly Dictionary<string, uint> _guiIndices = new Dictionary<string, uint>();

    private uint _nextIndex;
    
    public void Create(ICollection<string> paths){
        _nextIndex = 0;
        
        SKBitmap missing = SKBitmap.Decode("./Assets/Textures/missing.png");
        Textures.Add(missing);
        _nextIndex++;
        
        foreach (string path in paths) {
            SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/{path}.png");
            if (bitmap != null) {
                Textures.Add(bitmap);
                _blockIndices[path] = _nextIndex++;
            }
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
        return _blockIndices.TryGetValue(path, out uint value) ? value : 0;
    }
    
    public uint LookupGui(string path){
        return _guiIndices[path];
    }
}