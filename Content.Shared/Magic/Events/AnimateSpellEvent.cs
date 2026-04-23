using System;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Magic.Events;

[Serializable, NetSerializable]
public sealed partial class AnimateSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; } = InGameICChatType.Speak;

    [DataField]
    [NonSerialized]
    public ComponentRegistry AddComponents = new();

    [DataField]
    public HashSet<string> RemoveComponents = new();
}
