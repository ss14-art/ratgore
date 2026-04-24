using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared._Art.TTS;

/// <summary>
/// Prototype represent available TTS voices
/// </summary>
[Prototype("ttsVoice")]
// ReSharper disable once InconsistentNaming
public sealed partial class TTSVoicePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("name")]
    public string Name { get; set; } = string.Empty;

    [DataField("sex", required: true)]
    public Sex Sex { get; set; } = default!;

    [DataField("speaker", required: true)]
    public string Speaker { get; set; } = string.Empty;

    [DataField("roundStart")]
    public bool RoundStart { get; set; } = true;
}
