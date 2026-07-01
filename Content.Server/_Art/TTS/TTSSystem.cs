using System.Threading;
using System.Threading.Tasks;
using Content.Server.Language;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared._Art.CVars;
using Content.Shared._Art.TTS;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Art.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    private const int MaxMessageChars = 200;
    private bool _isEnabled;


    public override void Initialize()
    {
        _cfg.OnValueChanged(ArtCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke, after: [typeof(RadioSystem), typeof(HeadsetSystem)]); // Art-TTS

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _ttsManager.ResetCache());
        SubscribeLocalEvent<ActorComponent, TTSRadioPlayEvent>(OnTTSRadioPlayEvent);

        SubscribeNetworkEvent<RequestPreviewTTSEvent>(OnRequestPreviewTTS);
    }

    private async void OnEntitySpoke(EntityUid uid, TTSComponent component, EntitySpokeEvent args)
    {
        if (!_isEnabled || args.Message.Length > MaxMessageChars)
            return;

        if (args.RadioMessageSent)
            return;

        if (!args.Language.SpeechOverride.RequireSpeech)
            return;

        var voiceId = component.VoicePrototype;
        // var voiceEv = new TransformSpeakerVoiceEvent(uid, voiceId);
        // RaiseLocalEvent(uid, voiceEv);
        // voiceId = voiceEv.VoiceId;

        if (!_prototypeManager.TryIndex(voiceId, out var protoVoice))
            return;

        if (args.IsWhisper)
        {
            HandleWhisper(uid, args.Message, args.Language, protoVoice.Speaker);
            return;
        }

        HandleSay(uid, args.Message, args.Language, protoVoice.Speaker);
    }

    private void OnTTSRadioPlayEvent(EntityUid uid, ActorComponent comp, TTSRadioPlayEvent args)
    {
        if (!_isEnabled || args.Message.Length > MaxMessageChars)
            return;

        HandleReceiveRadio(uid, args.Message, args.Language, args.Voice);
    }

    private async void HandleReceiveRadio(EntityUid uid, string message, LanguagePrototype language, string speaker)
    {
        var recipients = Filter.Empty();
        var hasRecipients = false;

        foreach (var session in Filter.Pvs(uid).Recipients)
        {
            if (!session.AttachedEntity.HasValue)
                continue;

            EntityManager.TryGetComponent(session.AttachedEntity.Value, out LanguageSpeakerComponent? lang);
            if (!_language.CanUnderstand(new(session.AttachedEntity.Value, lang), language.ID))
                continue;

            recipients.AddPlayer(session);
            hasRecipients = true;
        }

        if (!hasRecipients)
            return;

        var soundData = await GenerateTTS(message, speaker, "radio");
        if (soundData is null) return;
        RaiseNetworkEvent(new PlayTTSEvent(soundData, GetNetEntity(uid)), recipients);
    }

    private async void HandleSay(EntityUid uid, string message, LanguagePrototype language, string speaker)
    {
        var normal = await GenerateTTS(message, speaker);
        if (normal is null)
            return;

        // var obfuscated = await GenerateTTS(_language.ObfuscateSpeech(message, language), speaker);
        // if (obfuscated is null)
        //     return;

        var nilter = Filter.Empty();
        var lilter = Filter.Empty();
        foreach (var session in Filter.Pvs(uid).Recipients)
        {
            if (!session.AttachedEntity.HasValue)
                continue;

            EntityManager.TryGetComponent(session.AttachedEntity.Value, out LanguageSpeakerComponent? lang);
            if (_language.CanUnderstand(new(session.AttachedEntity.Value, lang), language.ID))
                nilter.AddPlayer(session);
            else
                lilter.AddPlayer(session);
        }

        RaiseNetworkEvent(new PlayTTSEvent(normal, GetNetEntity(uid)), nilter);
        // RaiseNetworkEvent(new PlayTTSEvent(obfuscated, GetNetEntity(uid)), lilter, false);
    }

    private async void HandleWhisper(EntityUid uid, string message, LanguagePrototype language, string speaker)
    {
        var normal = await GenerateTTS(message, speaker);
        if (normal is null)
            return;

        // var obfuscated = await GenerateTTS(message, speaker);
        // if (obfuscated is null)
        //     return;

        // TODO: Check obstacles
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var nilter = Filter.Empty();
        var lilter = Filter.Empty();
        foreach (var session in Filter.Pvs(uid).Recipients)
        {
            if (!session.AttachedEntity.HasValue)
                continue;

            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();
            if (distance > ChatSystem.WhisperMuffledRange)
                continue;

            EntityManager.TryGetComponent(session.AttachedEntity.Value, out LanguageSpeakerComponent? lang);
            if (_language.CanUnderstand(new(session.AttachedEntity.Value, lang), language.ID)
                && distance <= ChatSystem.WhisperClearRange)
                nilter.AddPlayer(session);
            else
                lilter.AddPlayer(session);
        }

        RaiseNetworkEvent(new PlayTTSEvent(normal, GetNetEntity(uid), true), nilter);
        // RaiseNetworkEvent(new PlayTTSEvent(obfuscated, GetNetEntity(uid), true), lilter, false);
    }

    private readonly Dictionary<string, Task<byte[]?>> _ttsTasks = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    // ReSharper disable once InconsistentNaming
    private async Task<byte[]?> GenerateTTS(string text, string speaker, string? effect = null)
    {
        var textSanitized = Sanitize(text);
        if (string.IsNullOrEmpty(textSanitized))
            return null;

        if (char.IsLetter(textSanitized[^1]))
            textSanitized += ".";

        return await _ttsManager.ConvertTextToSpeech(speaker, textSanitized, effect);
        // var taskKey = $"{textSanitized}_{speaker}_{effect}";

        // await _lock.WaitAsync();
        // try
        // {
        //     if (_ttsTasks.TryGetValue(taskKey, out var existingTask))
        //         return await existingTask;

        //     var newTask = _ttsManager.ConvertTextToSpeech(speaker, textSanitized);
        //     _ttsTasks[taskKey] = newTask;
        // }
        // finally
        // {
        //     _lock.Release();
        // }

        // try
        // {
        //     return await _ttsTasks[taskKey];
        // }
        // finally
        // {
        //     await _lock.WaitAsync();
        //     try
        //     {
        //         _ttsTasks.Remove(taskKey);
        //     }
        //     finally
        //     {
        //         _lock.Release();
        //     }
        // }
    }
}
