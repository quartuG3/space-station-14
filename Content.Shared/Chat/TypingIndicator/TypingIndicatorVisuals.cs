using Robust.Shared.Serialization;

namespace Content.Shared.Chat.TypingIndicator;

[Serializable]
public enum TypingIndicatorState : byte
{
    None = 0,
    Typing,
    TypingQuestion,
    TypingAction,
    Thinking
}

[Serializable, NetSerializable]
public enum TypingIndicatorVisuals : byte
{
    State
}

[Serializable]
public enum TypingIndicatorLayers : byte
{
    Base
}
