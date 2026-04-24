using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Prototypes;

using Robust.Shared.Serialization;

namespace Content.Shared.Magic.Events;

[Serializable, NetSerializable]
public sealed partial class ProjectileSpellEvent : WorldTargetActionEvent, ISpeakSpell
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField]
    public string? Speech { get; private set; }

    public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
