using Content.Server.Popups;
using Content.Server.Storage.Components;
using Content.Server.Power.Components;
using Content.Server.Tools;
using Content.Server.Wires;
using Content.Server.Lock;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.APC;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Components;
using Content.Shared.Wires;
using Content.Shared.Verbs;
using Content.Shared.Power;
using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Server.Power.NodeGroups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Power.EntitySystems
{
    [UsedImplicitly]
    internal sealed class ApcSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ToolSystem _toolSystem = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly PowerCellSystem _cellSystem = default!;

        private const float ScrewTime = 2f;

        public override void Initialize()
        {
            base.Initialize();

            UpdatesAfter.Add(typeof(PowerNetSystem));

            SubscribeLocalEvent<ApcComponent, GetVerbsEvent<AlternativeVerb>>(OnApcAltVerb);

            SubscribeLocalEvent<ApcComponent, MapInitEvent>(OnApcInit);
            SubscribeLocalEvent<ApcComponent, PowerCellChangedEvent>(OnPowerCellChanged);
//            SubscribeLocalEvent<ApcComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
            SubscribeLocalEvent<ApcComponent, ApcToggleMainBreakerMessage>(OnToggleMainBreaker);
            SubscribeLocalEvent<ApcComponent, ApcToggleCoverMessage>(OnToggleCover);
            SubscribeLocalEvent<ApcComponent, ApcEquipmentChannelStateSetMessage>(OnEquipmentChannelStateChanged);
            SubscribeLocalEvent<ApcComponent, ApcLightingChannelStateSetMessage>(OnLightingChannelStateChanged);
            SubscribeLocalEvent<ApcComponent, ApcEnvironmentChannelStateSetMessage>(OnEnvironmentChannelStateChanged);
            SubscribeLocalEvent<ApcComponent, GotEmaggedEvent>(OnEmagged);

            SubscribeLocalEvent<ApcToolFinishedEvent>(OnToolFinished);
            SubscribeLocalEvent<ApcComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<ApcComponent, ExaminedEvent>(OnExamine);
        }

        private void OnPowerCellChanged(EntityUid uid, ApcComponent component, PowerCellChangedEvent args)
        {
            UpdateApcState(uid, component);
        }

        // Change the APC's state only when the battery state changes, or when it's first created.
        private void OnBatteryChargeChanged(EntityUid uid, ApcComponent component, ChargeChangedEvent args)
        {
            UpdateApcState(uid, component);
        }

        private void OnApcInit(EntityUid uid, ApcComponent component, MapInitEvent args)
        {
            UpdateApcState(uid, component);
        }
        private void OnToggleMainBreaker(EntityUid uid, ApcComponent component, ApcToggleMainBreakerMessage args)
        {
            TryComp<AccessReaderComponent>(uid, out var access);
            if (args.Session.AttachedEntity == null)
                return;

            if (access == null || _accessReader.IsAllowed(args.Session.AttachedEntity.Value, access))
            {
                ApcToggleBreaker(uid, component);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("apc-component-insufficient-access"),
                    args.Session, PopupType.Medium);
            }
        }

        public void ApcToggleBreaker(EntityUid uid, ApcComponent? apc = null, PowerNetworkBatteryComponent? battery = null)
        {
            if (!Resolve(uid, ref apc, ref battery))
                return;

            apc.MainBreakerEnabled = !apc.MainBreakerEnabled;
            battery.CanDischarge = apc.MainBreakerEnabled;

            UpdateUIState(uid, apc);
            SoundSystem.Play(apc.OnReceiveMessageSound.GetSound(), Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(-2f));
        }

        private void OnToggleCover(EntityUid uid, ApcComponent component, ApcToggleCoverMessage args)
        {
            TryComp<AccessReaderComponent>(uid, out var access);
            if (args.Session.AttachedEntity == null)
                return;

            if (access == null || _accessReader.IsAllowed(args.Session.AttachedEntity.Value, access))
            {
                ApcToggleCover(uid, component);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("apc-component-insufficient-access"),
                    args.Session, PopupType.Medium);
            }
        }

        public void ApcToggleCover(EntityUid uid, ApcComponent? apc = null, PowerNetworkBatteryComponent? battery = null)
        {
            if (!Resolve(uid, ref apc))
                return;

            if(apc.MaintaincePanelOpened)
                return;

            apc.MaintaincePanelUnlocked = !apc.MaintaincePanelUnlocked;

            UpdateUIState(uid, apc);
            SoundSystem.Play(apc.OnReceiveMessageSound.GetSound(), Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(-2f));
        }

        private ApcPowerChannelState ApcPowerChannelModeToState(ApcPowerChannelMode mode)
        {
            return mode switch
            {
                ApcPowerChannelMode.Off => ApcPowerChannelState.Off,
                ApcPowerChannelMode.On => ApcPowerChannelState.On,
                ApcPowerChannelMode.OnAuto => ApcPowerChannelState.OnAuto,
                _ => ApcPowerChannelState.Off
            };
        }

        private ApcPowerChannelMode ApcPowerChannelStateToMode(ApcPowerChannelState state)
        {
            return state switch
            {
                ApcPowerChannelState.Off => ApcPowerChannelMode.Off,
                ApcPowerChannelState.OffAuto => ApcPowerChannelMode.Off,
                ApcPowerChannelState.On => ApcPowerChannelMode.On,
                ApcPowerChannelState.OnAuto => ApcPowerChannelMode.OnAuto,
                _ => ApcPowerChannelMode.Off
            };
        }

        private void OnEquipmentChannelStateChanged(EntityUid uid, ApcComponent component, ApcEquipmentChannelStateSetMessage args)
        {
            TryComp<AccessReaderComponent>(uid, out var access);
            if (args.Session.AttachedEntity == null)
                return;

            if (access == null || _accessReader.IsAllowed(args.Session.AttachedEntity.Value, access))
            {
                ApcPowerChannelState state = ApcPowerChannelModeToState(args.Mode);
                ApcChangeEquipmentChannel(uid, state, component);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("apc-component-insufficient-access"),
                    args.Session, PopupType.Medium);
            }
        }

        public void ApcChangeEquipmentChannel(EntityUid uid, ApcPowerChannelState newstate, ApcComponent? apc = null, NodeContainerComponent? ncComp = null)
        {
            if (!Resolve(uid, ref apc, ref ncComp))
                return;

            if(apc.EquipmentChannelState != newstate)
            {
                apc.EquipmentChannelState = newstate;

                var netQ = ncComp.GetNode<Node>("output").NodeGroup as ApcNet;
                if(netQ != null)
                {
                    netQ.EquipmentEnabled = apc.EquipmentChannelState > ApcPowerChannelState.OffAuto;
                    var net = netQ!;
                    foreach(ApcPowerReceiverComponent receiver in netQ.AllReceivers)
                    {
                        if(receiver.PowerChannel == ApcPowerChannel.Equipment)
                            receiver.PowerDisabled = !net.EquipmentEnabled;
                    }
                }
            }

            UpdateUIState(uid, apc);
            SoundSystem.Play(apc.OnReceiveMessageSound.GetSound(), Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(-2f));
        }

        private void OnLightingChannelStateChanged(EntityUid uid, ApcComponent component, ApcLightingChannelStateSetMessage args)
        {
            TryComp<AccessReaderComponent>(uid, out var access);
            if (args.Session.AttachedEntity == null)
                return;

            if (access == null || _accessReader.IsAllowed(args.Session.AttachedEntity.Value, access))
            {
                ApcPowerChannelState state = ApcPowerChannelModeToState(args.Mode);
                ApcChangeLightingChannel(uid, state, component);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("apc-component-insufficient-access"),
                    args.Session, PopupType.Medium);
            }
        }

        public void ApcChangeLightingChannel(EntityUid uid, ApcPowerChannelState newstate, ApcComponent? apc = null, NodeContainerComponent? ncComp = null)
        {
            if (!Resolve(uid, ref apc, ref ncComp))
                return;

            if(apc.LightingChannelState != newstate)
            {
                apc.LightingChannelState = newstate;

                var netQ = ncComp.GetNode<Node>("output").NodeGroup as ApcNet;
                if(netQ != null)
                {
                    netQ.LightingEnabled = apc.LightingChannelState > ApcPowerChannelState.OffAuto;
                    var net = netQ!;
                    foreach(ApcPowerReceiverComponent receiver in netQ.AllReceivers)
                    {
                        if(receiver.PowerChannel == ApcPowerChannel.Lighting)
                            receiver.PowerDisabled = !net.LightingEnabled;
                    }
                }
            }

            UpdateUIState(uid, apc);
            SoundSystem.Play(apc.OnReceiveMessageSound.GetSound(), Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(-2f));
        }

        private void OnEnvironmentChannelStateChanged(EntityUid uid, ApcComponent component, ApcEnvironmentChannelStateSetMessage args)
        {
            TryComp<AccessReaderComponent>(uid, out var access);
            if (args.Session.AttachedEntity == null)
                return;

            if (access == null || _accessReader.IsAllowed(args.Session.AttachedEntity.Value, access))
            {
                ApcPowerChannelState state = ApcPowerChannelModeToState(args.Mode);
                ApcChangeEnvironmentChannel(uid, state, component);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("apc-component-insufficient-access"),
                    args.Session, PopupType.Medium);
            }
        }

        public void ApcChangeEnvironmentChannel(EntityUid uid, ApcPowerChannelState newstate, ApcComponent? apc = null, NodeContainerComponent? ncComp = null)
        {
            if (!Resolve(uid, ref apc, ref ncComp))
                return;

            if(apc.EnvironmentChannelState != newstate)
            {
                apc.EnvironmentChannelState = newstate;

                var netQ = ncComp.GetNode<Node>("output").NodeGroup as ApcNet;
                if(netQ != null)
                {
                    netQ.EnvironmentEnabled = apc.EnvironmentChannelState > ApcPowerChannelState.OffAuto;
                    var net = netQ!;

                    foreach(ApcPowerReceiverComponent receiver in net.AllReceivers)
                    {
                        if(receiver.PowerChannel == ApcPowerChannel.Environment)
                            receiver.PowerDisabled = !net.EnvironmentEnabled;
                    }
                }
            }

            UpdateUIState(uid, apc);
            SoundSystem.Play(apc.OnReceiveMessageSound.GetSound(), Filter.Pvs(uid), uid, AudioParams.Default.WithVolume(-2f));
        }

        private void OnEmagged(EntityUid uid, ApcComponent comp, GotEmaggedEvent args)
        {
            if(!comp.Emagged)
            {
                comp.Emagged = true;
                args.Handled = true;
            }
        }

        public void UpdateApcState(EntityUid uid,
            ApcComponent? apc=null,
            BatteryComponent? battery=null)
        {
            if (!Resolve(uid, ref apc))
                return;

            _cellSystem.TryGetBatteryFromSlot(uid, out battery);

            if (TryComp(uid, out AppearanceComponent? appearance))
            {
                UpdatePanelAppearance(uid, appearance, apc);
            }

            var newState = CalcChargeState(uid, apc, battery);
            if (newState != apc.LastChargeState && apc.LastChargeStateTime + ApcComponent.VisualsChangeDelay < _gameTiming.CurTime)
            {
                apc.LastChargeState = newState;
                apc.LastChargeStateTime = _gameTiming.CurTime;

                if (appearance != null)
                {
                    appearance.SetData(ApcVisuals.ChargeState, newState);
                }
            }

            var extPowerState = CalcExtPowerState(uid, apc, battery);
            if (extPowerState != apc.LastExternalState
                || apc.LastUiUpdate + ApcComponent.VisualsChangeDelay < _gameTiming.CurTime)
            {
                apc.LastExternalState = extPowerState;
                apc.LastUiUpdate = _gameTiming.CurTime;
                UpdateUIState(uid, apc, battery);
            }
        }

        public void UpdateUIState(EntityUid uid,
            ApcComponent? apc = null,
            BatteryComponent? battery = null,
            NodeContainerComponent? ncComp = null,
            ServerUserInterfaceComponent? ui = null)
        {
            if (!Resolve(uid, ref apc, ref ncComp, ref ui))
                return;

            _cellSystem.TryGetBatteryFromSlot(uid, out battery);

            float equipmentconsume = 0.0f;
            float lightingconsume = 0.0f;
            float environmentconsume = 0.0f;
            float totalconsume = 0.0f;

            var netQ = ncComp.GetNode<Node>("output").NodeGroup as ApcNet;
            if(netQ != null)
            {
                var net = netQ!;

                foreach(ApcPowerReceiverComponent receiver in net.AllReceivers)
                {
                    if (!receiver.Powered)
                    {
                        continue;
                    }

                    switch(receiver.PowerChannel)
                    {
                        case ApcPowerChannel.Equipment:
                        {
                            equipmentconsume += receiver.Load;
                            break;
                        }
                        case ApcPowerChannel.Lighting:
                        {
                            lightingconsume += receiver.Load;
                            break;
                        }
                        case ApcPowerChannel.Environment:
                        {
                            environmentconsume += receiver.Load;
                            break;
                        }
                        default:
                        {
                            totalconsume += receiver.Load;
                            break;
                        }
                    }
                }
            }

            totalconsume += equipmentconsume + lightingconsume + environmentconsume;

            if (_userInterfaceSystem.GetUiOrNull(uid, ApcUiKey.Key, ui) is { } bui)
            {
                bui.SetState(new ApcBoundInterfaceState(apc.MainBreakerEnabled, apc.MaintaincePanelUnlocked, apc.LastExternalState, battery == null ? 0 : battery.CurrentCharge / battery.MaxCharge, ApcPowerChannelStateToMode(apc.EquipmentChannelState), ApcPowerChannelStateToMode(apc.LightingChannelState), ApcPowerChannelStateToMode(apc.EnvironmentChannelState), equipmentconsume, lightingconsume, environmentconsume, totalconsume));
            }
        }

        public ApcChargeState CalcChargeState(EntityUid uid,
            ApcComponent? apc=null,
            BatteryComponent? battery=null)
        {
            if (!Resolve(uid, ref apc))
                return ApcChargeState.Lack;

            if(!_cellSystem.TryGetBatteryFromSlot(uid, out battery))
                return ApcChargeState.Lack;

            var chargeFraction = battery.CurrentCharge / battery.MaxCharge;

            if (chargeFraction > ApcComponent.HighPowerThreshold)
            {
                return ApcChargeState.Full;
            }

            var netBattery = Comp<PowerNetworkBatteryComponent>(uid);
            var delta = netBattery.CurrentSupply - netBattery.CurrentReceiving;

            return delta < 0 ? ApcChargeState.Charging : ApcChargeState.Lack;
        }

        public ApcExternalPowerState CalcExtPowerState(EntityUid uid,
            ApcComponent? apc=null,
            BatteryComponent? battery=null)
        {
            if (!Resolve(uid, ref apc))
                return ApcExternalPowerState.None;

            if(!_cellSystem.TryGetBatteryFromSlot(uid, out battery))
                return ApcExternalPowerState.None;

            var netBat = Comp<PowerNetworkBatteryComponent>(uid);
            if (netBat.CurrentReceiving == 0 && !MathHelper.CloseTo(battery.CurrentCharge / battery.MaxCharge, 1))
            {
                return ApcExternalPowerState.None;
            }

            var delta = netBat.CurrentReceiving - netBat.CurrentSupply;
            if (!MathHelper.CloseToPercent(delta, 0, 0.1f) && delta < 0)
            {
                return ApcExternalPowerState.Low;
            }

            return ApcExternalPowerState.Good;
        }

        public static ApcPanelState GetPanelState(ApcComponent apc)
        {
            if (apc.MaintaincePanelOpened)
                return ApcPanelState.Maintaince;

            if(apc.Emagged)
                return ApcPanelState.Emag;

            if (apc.IsApcOpen)
                return ApcPanelState.Open;
            else
                return ApcPanelState.Closed;
        }

        public static ApcLockState GetLockState(ApcComponent apc)
        {
            return apc.MaintaincePanelUnlocked switch
            {
                false => ApcLockState.Locked,
                true => ApcLockState.Unlocked
            };
        }

        private void OnApcAltVerb(EntityUid uid, ApcComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if(component.MaintaincePanelUnlocked)
            {
                args.Verbs.Add(new AlternativeVerb()
                {
                    Text = Loc.GetString(component.MaintaincePanelOpened ? "apc-component-cover-close" : "apc-component-cover-open"),
                    Act = () => ApcVerbToggleCover(uid, component),
                });
            }
        }

        private void ApcVerbToggleCover(EntityUid uid, ApcComponent component)
        {
            component.MaintaincePanelOpened = !component.MaintaincePanelOpened;

            if (TryComp<ItemSlotsComponent>(uid, out var itemSlots))
            {
                _itemSlotsSystem.SetLock(uid, itemSlots.Slots["cell_slot"], !component.MaintaincePanelOpened);
            }
        }

        private void OnInteractUsing(EntityUid uid, ApcComponent component, InteractUsingEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Used, out ToolComponent? tool))
                return;
            if (_toolSystem.UseTool(args.Used, args.User, uid, 0f, ScrewTime, new string[] { "Screwing" }, doAfterCompleteEvent: new ApcToolFinishedEvent(uid), toolComponent: tool))
            {
                args.Handled = true;
            }
        }

        private void OnToolFinished(ApcToolFinishedEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Target, out ApcComponent? component))
                return;
            component.IsApcOpen = !component.IsApcOpen;

            if (TryComp(args.Target, out AppearanceComponent? appearance))
            {
                UpdatePanelAppearance(args.Target, appearance);
            }

            if (component.IsApcOpen)
            {
                SoundSystem.Play(component.ScrewdriverOpenSound.GetSound(), Filter.Pvs(args.Target), args.Target);
            }
            else
            {
                SoundSystem.Play(component.ScrewdriverCloseSound.GetSound(), Filter.Pvs(args.Target), args.Target);
            }
        }

        private void UpdatePanelAppearance(EntityUid uid, AppearanceComponent? appearance = null, ApcComponent? apc = null)
        {
            if (!Resolve(uid, ref appearance, ref apc, false))
                return;

            appearance.SetData(ApcVisuals.PanelState, GetPanelState(apc));
            appearance.SetData(ApcVisuals.EquipmentChannelState, apc.EquipmentChannelState);
            appearance.SetData(ApcVisuals.LightingChannelState, apc.LightingChannelState);
            appearance.SetData(ApcVisuals.EnvironmentChannelState, apc.EnvironmentChannelState);
            appearance.SetData(ApcVisuals.LockState, GetLockState(apc));
        }

        private sealed class ApcToolFinishedEvent : EntityEventArgs
        {
            public EntityUid Target { get; }

            public ApcToolFinishedEvent(EntityUid target)
            {
                Target = target;
            }
        }

        private void OnExamine(EntityUid uid, ApcComponent component, ExaminedEvent args)
        {
            args.PushMarkup(Loc.GetString(component.IsApcOpen
                ? "apc-component-on-examine-panel-open"
                : "apc-component-on-examine-panel-closed"));
        }
    }
}
