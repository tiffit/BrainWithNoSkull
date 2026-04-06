namespace BrainWithNoSkull.Registries;

public class Registry<T> where T : IRegistryEntry {

    private readonly Dictionary<string, T> _entries = new Dictionary<string, T>();
    private readonly List<T> _indexedEntries = [];

    public TI Register<TI>(TI entry) where TI : T{
        _entries[entry.GetId()] = entry;
        if (entry.GetIndex() != _indexedEntries.Count) {
            throw new ArgumentException("Index misalignment");
        }
        _indexedEntries.Add(entry);
        return entry;
    }

    public T? GetEntry(string id){
        _entries.TryGetValue(id, out T? result);
        return result;
    }
    
    public T? GetEntry(ushort index){
        return _indexedEntries[index];
    }

    public IEnumerable<T> GetEntries(){
        return _entries.Values;
    }

}

public class Registries {
    public static void RegisterAll(){
        BlockRegistry.Init();
        ItemRegistry.Init();
        EntityTypeRegistry.Init();
        RecipeRegistry.Init();
    }
}