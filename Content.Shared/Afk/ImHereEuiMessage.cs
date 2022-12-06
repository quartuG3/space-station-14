using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Afk
{
    [Serializable, NetSerializable]
    public sealed class ImHereMessage : EuiMessageBase
    {
        public ImHereMessage()
        {
        }
    }
}
