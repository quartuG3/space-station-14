using Content.Client.Eui;
using Content.Shared.Afk;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.Afk.UI
{
    [UsedImplicitly]
    public sealed class AfkCheckEui : BaseEui
    {
        private readonly AfkCheckWindow _window;

        public AfkCheckEui()
        {
            _window = new AfkCheckWindow();

            _window.ImOkButton.OnPressed += _ =>
            {
                SendMessage(new ImHereMessage());
                _window.Close();
            };
        }

        public override void Opened()
        {
            IoCManager.Resolve<IClyde>().RequestWindowAttention();
            _window.OpenCentered();
        }

        public override void Closed()
        {
            _window.Close();
        }
    }
}
