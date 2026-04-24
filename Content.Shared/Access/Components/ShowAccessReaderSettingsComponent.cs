using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Access.Components;

/// <summary>
/// Allows seeing whether an access reader's settings have been modified.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowAccessReaderSettingsComponent : Component, IClothingSlots
{
    public SlotFlags Slots { get; set; } = ~SlotFlags.POCKET;
}
