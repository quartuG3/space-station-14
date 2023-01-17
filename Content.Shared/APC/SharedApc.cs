using Robust.Shared.Serialization;

namespace Content.Shared.APC
{
    [Serializable, NetSerializable]
    public enum ApcVisuals
    {
        /// <summary>
        ///     APC lights/HUD.
        /// </summary>
        ChargeState,

        EquipmentChannelState,
        LightingChannelState,
        EnvironmentChannelState,

        /// <summary>
        ///     APC lock.
        /// </summary>
        LockState,

        /// <summary>
        ///     APC frame.
        /// </summary>
        PanelState
    }

    [Serializable, NetSerializable]
    public enum ApcPanelState
    {
        /// <summary>
        ///     APC is closed.
        /// </summary>
        Closed,
        /// <summary>
        ///     APC wire panel opened.
        /// </summary>
        Open,
        /// <summary>
        ///     APC cover open. Allow for battery replace.
        /// </summary>
        Maintaince,
        /// <summary>
        /// APC is emagged (and not displaying other useful power colors at a glance)
        /// </summary>
        Emag
    }

    [Serializable, NetSerializable]
    public enum ApcChargeState
    {
        /// <summary>
        ///     APC does not have enough power to charge cell (if necessary) and keep powering the area.
        /// </summary>
        Lack,

        /// <summary>
        ///     APC is not full but has enough power.
        /// </summary>
        Charging,

        /// <summary>
        ///     APC battery is full and has enough power.
        /// </summary>
        Full,
    }

    [Serializable, NetSerializable]
    public enum ApcLockState
    {
        /// <summary>
        ///     APC fully locked.
        /// </summary>
        Locked,

        /// <summary>
        ///     APC lock is unlocked.
        /// </summary>
        Unlocked
    }

    [Serializable, NetSerializable]
    public sealed class ApcBoundInterfaceState : BoundUserInterfaceState
    {
        public readonly bool MainBreaker;
        public readonly bool CoverButton;
        public readonly ApcExternalPowerState ApcExternalPower;
        public readonly float Charge;
        public readonly ApcPowerChannelMode Equipment;
        public readonly ApcPowerChannelMode Lighting;
        public readonly ApcPowerChannelMode Environment;
        public readonly float EquipmentConsume;
        public readonly float LightingConsume;
        public readonly float EnvironmentConsume;
        public readonly float TotalConsume;

        public ApcBoundInterfaceState(bool mainBreaker, bool coverButton, ApcExternalPowerState apcExternalPower, float charge, ApcPowerChannelMode equipment, ApcPowerChannelMode lighting, ApcPowerChannelMode environment, float equipmentconsume, float lightingconsume, float environmentconsume, float totalconsume)
        {
            MainBreaker = mainBreaker;
            CoverButton = coverButton;
            ApcExternalPower = apcExternalPower;
            Charge = charge;
            Equipment = equipment;
            Lighting = lighting;
            Environment = environment;
            EquipmentConsume = equipmentconsume;
            LightingConsume = lightingconsume;
            EnvironmentConsume = environmentconsume;
            TotalConsume = totalconsume;
        }
    }

    [NetSerializable, Serializable]
    public enum ApcPowerChannel
    {
        Equipment,
        Lighting,
        Environment
    }

    [NetSerializable, Serializable]
    public enum ApcPowerChannelState
    {
        Off,
        OffAuto,
        On,
        OnAuto
    }

    [NetSerializable, Serializable]
    public enum ApcPowerChannelMode
    {
        Off,
        On,
        OnAuto
    }

    public enum ApcExternalPowerState
    {
        None,
        Low,
        Good,
    }

    [NetSerializable, Serializable]
    public enum ApcUiKey
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class ApcToggleMainBreakerMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class ApcToggleCoverMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class ApcEquipmentChannelStateSetMessage : BoundUserInterfaceMessage
    {
        public ApcPowerChannelMode Mode { get; }

        public ApcEquipmentChannelStateSetMessage(ApcPowerChannelMode mode)
        {
            Mode = mode;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ApcLightingChannelStateSetMessage : BoundUserInterfaceMessage
    {
        public ApcPowerChannelMode Mode { get; }

        public ApcLightingChannelStateSetMessage(ApcPowerChannelMode mode)
        {
            Mode = mode;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ApcEnvironmentChannelStateSetMessage : BoundUserInterfaceMessage
    {
        public ApcPowerChannelMode Mode { get; }

        public ApcEnvironmentChannelStateSetMessage(ApcPowerChannelMode mode)
        {
            Mode = mode;
        }
    }
}
