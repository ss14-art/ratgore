using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Handles sending order requests to cargo. Doesn't handle orders themselves via shuttle or telepads.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CargoOrderConsoleComponent : Component
{
    [DataField("soundError")] public SoundSpecifier ErrorSound =
        new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    [DataField("soundConfirm")]
    public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    [DataField]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(2);

    public TimeSpan NextPrintTime;

    /// <summary>
    /// All of the <see cref="CargoProductPrototype.Group"/>s that are supported.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<string> AllowedGroups = new() { "market", "suzerai", "freeport", "commie", "spacer", "pangtai", "shinohara", "ship" };

    /// <summary>
    /// Radio channel on which order approval announcements are transmitted
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<RadioChannelPrototype> AnnouncementChannel = "Supply";

    [DataField]
    public ProtoId<CargoAccountPrototype> Account = "market";

    [DataField]
    public CargoOrderConsoleMode Mode = CargoOrderConsoleMode.DirectOrder;

    [DataField]
    public SoundSpecifier? ScanSound;

    public TimeSpan NextDenySoundTime;

    [DataField]
    public TimeSpan DenySoundDelay = TimeSpan.FromSeconds(0.5);

    public static ProtoId<RadioChannelPrototype> BaseAnnouncementChannel = "Supply";
}

[Serializable, NetSerializable]
public enum CargoOrderConsoleMode : byte
{
    DirectOrder,
    PrintSlip,
    SendToPrimary
}

