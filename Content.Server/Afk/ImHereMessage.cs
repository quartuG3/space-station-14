using Content.Server.EUI;
using Content.Shared.Afk;
using Content.Shared.Eui;
using Robust.Server.Player;

namespace Content.Server.Afk
{
    public sealed class AfkCheckEui : BaseEui
    {
        [Dependency] private readonly IAfkManager _afkManager = default!;
        private readonly IPlayerSession _player;

        public AfkCheckEui(IPlayerSession player)
        {
            _player = player;
        }

	public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not ImHereMessage message)
            {
                Close();
                return;
            }

            _afkManager.PlayerDidAction(_player);
            Close();
        }
    }
}
