using Content.Client.Power.APC.UI;
using Content.Shared.APC;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Client.Power.APC
{
    [UsedImplicitly]
    public sealed class ApcBoundUserInterface : BoundUserInterface
    {
        [ViewVariables] private ApcMenu? _menu;

        protected override void Open()
        {
            base.Open();

            _menu = new ApcMenu(this);
            _menu.OnClose += Close;
            _menu.OpenCentered();

            _menu.ApcEquipmentChannelStateChanged += OnApcEquipmentChannelStateChanged;
            _menu.ApcLightingChannelStateChanged += OnApcLightingChannelStateChanged;
            _menu.ApcEnvironmentChannelStateChanged += OnApcEnvironmentChannelStateChanged;
        }

        public ApcBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (ApcBoundInterfaceState) state;
            _menu?.UpdateState(castState);
        }

        public void BreakerPressed()
        {
            SendMessage(new ApcToggleMainBreakerMessage());
        }

        public void CoverPressed()
        {
            SendMessage(new ApcToggleCoverMessage());
        }

        private void OnApcEquipmentChannelStateChanged(ApcPowerChannelMode mode)
        {
            SendMessage(new ApcEquipmentChannelStateSetMessage(mode));
        }

        private void OnApcLightingChannelStateChanged(ApcPowerChannelMode mode)
        {
            SendMessage(new ApcLightingChannelStateSetMessage(mode));
        }

        private void OnApcEnvironmentChannelStateChanged(ApcPowerChannelMode mode)
        {
            SendMessage(new ApcEnvironmentChannelStateSetMessage(mode));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _menu?.Dispose();
            }
        }
    }
}
