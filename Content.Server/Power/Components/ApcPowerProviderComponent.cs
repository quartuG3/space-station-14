using Content.Server.Power.NodeGroups;
using Content.Shared.APC;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    [ComponentProtoName("PowerProvider")]
    public sealed class ApcPowerProviderComponent : BaseApcNetComponent
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public List<ApcPowerReceiverComponent> LinkedReceivers { get; } = new();

        public bool IsChannelEnabled(ApcPowerChannel channel)
        {
            if(Net != null)
                return Net.IsChannelEnabled(channel);
            return false;
        }

        public void AddReceiver(ApcPowerReceiverComponent receiver)
        {
            LinkedReceivers.Add(receiver);
            receiver.NetworkLoad.LinkedNetwork = default;
            if( Net != null )
            {
                receiver.PowerDisabled = Net.IsChannelEnabled(receiver.PowerChannel);
            }

            Net?.QueueNetworkReconnect();
        }

        public void RemoveReceiver(ApcPowerReceiverComponent receiver)
        {
            LinkedReceivers.Remove(receiver);
            receiver.NetworkLoad.LinkedNetwork = default;
            receiver.PowerDisabled = false;

            Net?.QueueNetworkReconnect();
        }

        protected override void AddSelfToNet(IApcNet apcNet)
        {
            apcNet.AddPowerProvider(this);
        }

        protected override void RemoveSelfFromNet(IApcNet apcNet)
        {
            apcNet.RemovePowerProvider(this);
        }
    }
}
