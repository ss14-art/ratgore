using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Serialization;


namespace Content.Shared.Magic.Events;

// TODO: Can probably just be an entity or something
[Serializable, NetSerializable]
public sealed partial class TeleportSpellEvent : WorldTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    // TODO: Move to magic component
    // TODO: Maybe not since sound specifier is a thing
    // Keep here to remind what the volume was set as
    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField]
    public float BlinkVolume = 5f;

    public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
