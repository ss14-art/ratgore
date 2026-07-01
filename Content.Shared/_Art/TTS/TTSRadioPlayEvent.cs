using Content.Shared.Language;
using Content.Shared.Chat;

namespace Content.Shared._Art.TTS;

public sealed class TTSRadioPlayEvent(ChatMessage originalChatMsg, string message, LanguagePrototype language, string voice) : EntityEventArgs
{
    public ChatMessage OriginalChatMsg { get; } = originalChatMsg;
    public string Message { get; } = message;
    public LanguagePrototype Language { get; } = language;
    public string Voice { get; } = voice;
}
