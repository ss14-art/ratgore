using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Chat;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.Magic.Events;

[Serializable, NetSerializable]
public sealed partial class RandomGlobalSpawnSpellEvent : WorldTargetActionEvent, ISpeakSpell
{
    [DataField]
    public List<EntitySpawnEntry>? Spawns;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public string? Speech { get; set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; } = InGameICChatType.Speak;
}

[Serializable, NetSerializable]
public sealed partial class MindSwapSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public TimeSpan TargetStunDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan PerformerStunDuration = TimeSpan.FromSeconds(1);

    [DataField]
    public string? Speech { get; set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; } = InGameICChatType.Speak;
}
