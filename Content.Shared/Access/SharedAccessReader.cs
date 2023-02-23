using Robust.Shared.Serialization;

namespace Content.Shared.Access.Systems
{
    /// <summary>
    /// Key representing which <see cref="BoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum AccessReaderUiKey : byte
    {
        Key,
    }

    /// <summary>
    /// Represents an <see cref="AgentIDCardComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class AccessReaderBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string[]? AccessList;
        public readonly bool Inverted;

        public AccessReaderBoundUserInterfaceState(string[]? accessList,bool inverted)
        {
            AccessList = accessList;
            Inverted = inverted;
        }
    }

    [Serializable, NetSerializable]
    public sealed class WriteToTargetAccessReaderMessage : BoundUserInterfaceMessage
    {
        public readonly List<string> AccessList;
        public readonly bool Inverted;

        public WriteToTargetAccessReaderMessage(List<string> accessList, bool inverted)
        {
            AccessList = accessList;
            Inverted = inverted;
        }
    }
}
