using Content.Shared.Mind;
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class BlobRuleComponent : Component
{
    [DataField]
    public SoundSpecifier? DetectedAudio = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/blob_detected.ogg");

    [DataField]
    public SoundSpecifier? CriticalAudio = new SoundPathSpecifier("/Audio/StationEvents/blobin_time.ogg");

    [ViewVariables]
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new();

    [ViewVariables]
    public BlobStage Stage = BlobStage.Default;

    [ViewVariables]
    public float Accumulator = 0f;
}

public enum BlobStage : byte
{
    Default,
    Begin,
    Critical,
    TheEnd,
}
