using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.World.BlockEntities;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class ContainerSyncRegistry {
    public static readonly Registry<ContainerSyncType> Registry = new Registry<ContainerSyncType>();

    public static ContainerSyncType Machine;

    public static void Init(){
        Machine = Registry.Register(new ContainerSyncTypeBuilder(typeof(MachineContainerSync), "Machine",
                (id, container, player) => new MachineContainerSync(id, container, player))
            .Build());
    }
}