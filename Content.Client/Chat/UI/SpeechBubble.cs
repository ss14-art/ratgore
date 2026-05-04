using System;
using System.Numerics;
using Content.Client.Chat.Managers;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Chat.UI
{
    public abstract class SpeechBubble : Control
    {
        protected const int FancyBubbleContentTopMargin = 10;

        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] protected readonly IConfigurationManager ConfigManager = default!;

        private readonly SharedTransformSystem _transformSystem;

        public enum SpeechType : byte
        {
            Emote,
            Say,
            Whisper,
            Looc
        }

        /// <summary>
        ///     The total time a speech bubble stays on screen.
        /// </summary>
        private const float TotalTime = 4;

        /// <summary>
        ///     The amount of time at the end of the bubble's life at which it starts fading.
        /// </summary>
        private const float FadeTime = 0.25f;

        /// <summary>
        ///     The distance in world space to offset the speech bubble from the center of the entity.
        ///     i.e. greater -> higher above the mob's head.
        /// </summary>
        private const float EntityVerticalOffset = 0.5f;

        /// <summary>
        ///     The default maximum width for speech bubbles.
        /// </summary>
        public const float SpeechMaxWidth = 256;

        private readonly EntityUid _senderEntity;

        /// <summary>
        /// Root panel returned by <see cref="BuildBubble"/>; used to re-measure after the control is in the UI tree.
        /// </summary>
        private readonly Control _bubbleRoot;

        /// <summary>
        /// Rich text layout can settle after attach / styles; refresh metrics for a few frames so wrapped lines fit.
        /// </summary>
        private int _layoutMetricsRefreshTicks = 4;

        private float _timeLeft = TotalTime;

        public float VerticalOffset { get; set; }
        private float _verticalOffsetAchieved;

        public Vector2 ContentSize { get; private set; }

        // man down
        public event Action<EntityUid, SpeechBubble>? OnDied;

        public static SpeechBubble CreateSpeechBubble(SpeechType type, ChatMessage message, EntityUid senderEntity)
        {
            switch (type)
            {
                case SpeechType.Emote:
                    return new TextSpeechBubble(message, senderEntity, "emoteBox");

                case SpeechType.Say:
                    return new FancyTextSpeechBubble(message, senderEntity, "sayBox");

                case SpeechType.Whisper:
                    return new FancyTextSpeechBubble(message, senderEntity, "whisperBox");

                case SpeechType.Looc:
                    return new TextSpeechBubble(message, senderEntity, "emoteBox", Color.FromHex("#48d1cc"));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
        {
            IoCManager.InjectDependencies(this);
            _senderEntity = senderEntity;
            _transformSystem = _entityManager.System<SharedTransformSystem>();

            // Use text clipping so new messages don't overlap old ones being pushed up.
            RectClipContent = true;

            var bubble = BuildBubble(message, speechStyleClass, fontColor);
            _bubbleRoot = bubble;

            AddChild(bubble);

            ForceRunStyleUpdate();

            bubble.Measure(Vector2Helpers.Infinity);
            ContentSize = bubble.DesiredSize;
            _verticalOffsetAchieved = -ContentSize.Y;

            Timer.Spawn(0, () =>
            {
                if (!Disposed)
                    RefreshBubbleLayoutMetrics();
            });
        }

        protected abstract Control BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null);

        private void RefreshBubbleLayoutMetrics()
        {
            if (Disposed || _bubbleRoot.Disposed)
                return;

            _bubbleRoot.InvalidateMeasure();
            _bubbleRoot.Measure(Vector2Helpers.Infinity);
            ContentSize = _bubbleRoot.DesiredSize;
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            _timeLeft -= args.DeltaSeconds;
            if (_entityManager.Deleted(_senderEntity) || _timeLeft <= 0)
            {
                // Timer spawn to prevent concurrent modification exception.
                Timer.Spawn(0, Die);
                return;
            }

            // Lerp to our new vertical offset if it's been modified.
            if (MathHelper.CloseToPercent(_verticalOffsetAchieved - VerticalOffset, 0, 0.1))
            {
                _verticalOffsetAchieved = VerticalOffset;
            }
            else
            {
                _verticalOffsetAchieved = MathHelper.Lerp(_verticalOffsetAchieved, VerticalOffset, 10 * args.DeltaSeconds);
            }

            if (!_entityManager.TryGetComponent<TransformComponent>(_senderEntity, out var xform) || xform.MapID != _eyeManager.CurrentMap)
            {
                Modulate = Color.White.WithAlpha(0);
                return;
            }

            if (_timeLeft <= FadeTime)
            {
                // Update alpha if we're fading.
                Modulate = Color.White.WithAlpha(_timeLeft / FadeTime);
            }
            else
            {
                // Make opaque otherwise, because it might have been hidden before
                Modulate = Color.White;
            }

            if (_layoutMetricsRefreshTicks > 0)
            {
                _layoutMetricsRefreshTicks--;
                RefreshBubbleLayoutMetrics();
            }

            var baseOffset = 0f;

            if (_entityManager.TryGetComponent<SpeechComponent>(_senderEntity, out var speech))
                baseOffset = speech.SpeechBubbleOffset;

            var offset = (-_eyeManager.CurrentEye.Rotation).ToWorldVec() * -(EntityVerticalOffset + baseOffset);
            var worldPos = _transformSystem.GetWorldPosition(xform) + offset;

            var lowerCenter = _eyeManager.WorldToScreen(worldPos) / UIScale;
            var screenPos = lowerCenter - new Vector2(ContentSize.X / 2, ContentSize.Y + _verticalOffsetAchieved);
            // Round to nearest 0.5
            screenPos = (screenPos * 2).Rounded() / 2;
            LayoutContainer.SetPosition(this, screenPos);

            // Always reserve full bubble size. Using a smaller height clips multi-line text because RectClipContent is true.
            SetWidth = ContentSize.X;
            SetHeight = ContentSize.Y;
        }

        private void Die()
        {
            if (Disposed)
            {
                return;
            }

            OnDied?.Invoke(_senderEntity, this);
        }

        /// <summary>
        ///     Causes the speech bubble to start fading IMMEDIATELY.
        /// </summary>
        public void FadeNow()
        {
            if (_timeLeft > FadeTime)
            {
                _timeLeft = FadeTime;
            }
        }

        protected FormattedMessage FormatSpeech(string message, Color? fontColor = null)
        {
            var msg = new FormattedMessage();
            if (fontColor != null)
                msg.PushColor(fontColor.Value);
            msg.AddMarkup(message);
            return msg;
        }

        protected FormattedMessage ExtractAndFormatSpeechSubstring(ChatMessage message, string tag, Color? fontColor = null)
        {
            return FormatSpeech(SharedChatSystem.GetStringInsideTag(message, tag), fontColor);
        }

        protected static bool IsShoutSpeech(ChatMessage message)
        {
            var raw = SharedChatSystem.GetStringInsideTag(message, "BubbleContent");
            if (string.IsNullOrEmpty(raw))
                raw = message.Message;

            var plain = FormattedMessage.RemoveMarkupPermissive(raw);
            return plain.TrimEnd().EndsWith("!!", StringComparison.Ordinal);
        }

    }

    public sealed class TextSpeechBubble : SpeechBubble
    {
        public TextSpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
            : base(message, senderEntity, speechStyleClass, fontColor)
        {
        }

        protected override Control BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null)
        {
            var shout = IsShoutSpeech(message);
            var pad = shout ? new Thickness(4, 4, 4, 4) : new Thickness(0);

            var label = new RichTextLabel
            {
                MaxWidth = SpeechMaxWidth,
                Margin = pad,
            };

            label.SetMessage(FormatSpeech(message.WrappedMessage, fontColor));

            var panel = new PanelContainer
            {
                StyleClasses = { "speechBox", speechStyleClass },
                Children = { label },
                ModulateSelfOverride = Color.White.WithAlpha(0.75f)
            };

            return panel;
        }
    }

    public sealed class FancyTextSpeechBubble : SpeechBubble
    {

        public FancyTextSpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
            : base(message, senderEntity, speechStyleClass, fontColor)
        {
        }

        protected override Control BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null)
        {
            var shout = IsShoutSpeech(message);

            if (!ConfigManager.GetCVar(CCVars.ChatEnableFancyBubbles))
            {
                var label = new RichTextLabel
                {
                    MaxWidth = SpeechMaxWidth,
                    Margin = shout ? new Thickness(4, 4, 4, 4) : new Thickness(0),
                };

                label.SetMessage(ExtractAndFormatSpeechSubstring(message, "BubbleContent", fontColor));

                var unfanciedPanel = new PanelContainer
                {
                    StyleClasses = { "speechBox", speechStyleClass },
                    Children = { label },
                    ModulateSelfOverride = Color.White.WithAlpha(0.75f)
                };
                return unfanciedPanel;
            }

            var bubbleHeader = new RichTextLabel
            {
                Margin = new Thickness(1, 1, 1, 1),
                MaxWidth = SpeechMaxWidth,
                VerticalAlignment = VAlignment.Top,
            };

            var contentMargin = shout
                ? new Thickness(4, FancyBubbleContentTopMargin + 1, 4, 4)
                : new Thickness(2, FancyBubbleContentTopMargin, 2, 2);

            var bubbleContent = new RichTextLabel
            {
                MaxWidth = SpeechMaxWidth,
                Margin = contentMargin,
                StyleClasses = { "bubbleContent" },
                VerticalAlignment = VAlignment.Top,
            };

            //We'll be honest. *Yes* this is hacky. Doing this in a cleaner way would require a bottom-up refactor of how saycode handles sending chat messages. -Myr
            bubbleHeader.SetMessage(ExtractAndFormatSpeechSubstring(message, "BubbleHeader", fontColor));
            bubbleContent.SetMessage(ExtractAndFormatSpeechSubstring(message, "BubbleContent", fontColor));

            //As for below: Some day this could probably be converted to xaml. But that is not today. -Myr
            var mainPanelMargin = shout
                ? new Thickness(6, 5, 6, 4)
                : new Thickness(4, 4, 4, 2);

            var mainPanel = new PanelContainer
            {
                StyleClasses = { "speechBox", speechStyleClass },
                Children = { bubbleContent },
                ModulateSelfOverride = Color.White.WithAlpha(0.75f),
                HorizontalAlignment = HAlignment.Center,
                Margin = mainPanelMargin,
            };

            var headerPanel = new PanelContainer
            {
                StyleClasses = { "speechBox", speechStyleClass },
                Children = { bubbleHeader },
                ModulateSelfOverride = Color.White.WithAlpha(ConfigManager.GetCVar(CCVars.ChatFancyNameBackground) ? 0.75f : 0f),
                HorizontalAlignment = HAlignment.Center,
            };

            // Stack header above body so measured height includes both (overlapping PanelContainers used Max per-axis and clipped wraps).
            var panel = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                SeparationOverride = -2,
                HorizontalAlignment = HAlignment.Center,
                Children = { headerPanel, mainPanel },
            };

            return panel;
        }
    }
}
