using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Present on cargo shuttles to provide metadata such as preventing spam calling.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class CargoShuttleComponent : Component
{
    /*
     * Still needed for drone console for now.
     */

    [ViewVariables(VVAccess.ReadWrite), DataField("nextCall")]
    public TimeSpan NextCall;

    [ViewVariables(VVAccess.ReadWrite), DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);
}
