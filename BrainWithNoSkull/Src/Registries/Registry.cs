namespace BrainWithNoSkull.Registries;

public class Registry<T> where T : IRegistryEntry {

    private readonly Dictionary<string, T> _entries = new Dictionary<string, T>();

    public TI Register<TI>(TI entry) where TI : T{
        _entries[entry.GetId()] = entry;
        return entry;
    }

    public T? GetEntry(string id){
        _entries.TryGetValue(id, out T? result);
        return result;
    }

}

public class Registries {
    public static void RegisterAll(){
        BlockRegistry.Init();
    }
}