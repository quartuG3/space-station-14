using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared.Examine;
using Content.Shared.PowerCell.Components;
using JetBrains.Annotations;

namespace Content.Server.Power.EntitySystems
{
    [UsedImplicitly]
    public sealed class BatterySystem : EntitySystem
    {
        [Dependency] private readonly PowerCellSystem _cellSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ExaminableBatteryComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<BatteryComponent, PriceCalculationEvent>(CalculateBatteryPrice);

            SubscribeLocalEvent<NetworkBatteryPreSync>(PreSync);
            SubscribeLocalEvent<NetworkBatteryPostSync>(PostSync);
        }

        private void OnExamine(EntityUid uid, ExaminableBatteryComponent component, ExaminedEvent args)
        {
            if (!TryComp<BatteryComponent>(uid, out var batteryComponent) && !_cellSystem.TryGetBatteryFromSlot(uid, out batteryComponent))
                return;

            if (args.IsInDetailsRange)
            {
                var effectiveMax = batteryComponent.MaxCharge;
                if (effectiveMax == 0)
                    effectiveMax = 1;
                var chargeFraction = batteryComponent.CurrentCharge / effectiveMax;
                var chargePercentRounded = (int) (chargeFraction * 100);
                args.PushMarkup(
                    Loc.GetString(
                        "examinable-battery-component-examine-detail",
                        ("percent", chargePercentRounded),
                        ("markupPercentColor", "green")
                    )
                );
            }
        }

        private void PreSync(NetworkBatteryPreSync ev)
        {
            foreach (var (netBat, bat) in EntityManager.EntityQuery<PowerNetworkBatteryComponent, BatteryComponent>())
            {
                netBat.NetworkBattery.Capacity = bat.MaxCharge;
                netBat.NetworkBattery.CurrentStorage = bat.CurrentCharge;
            }

            foreach (var (netBat, cell) in EntityManager.EntityQuery<PowerNetworkBatteryComponent, PowerCellSlotComponent>())
            {
                if(_cellSystem.TryGetBatteryFromSlot(netBat.Owner, out var bat, cell))
                {
                    netBat.NetworkBattery.Enabled = true;
                    netBat.NetworkBattery.Capacity = bat.MaxCharge;
                    netBat.NetworkBattery.MaxChargeRate = bat.ChargeRate;
                    netBat.NetworkBattery.SupplyRampRate = bat.MaxCharge / 2;
                    netBat.NetworkBattery.CurrentStorage = bat.CurrentCharge;
                }
                else
                {
                    netBat.NetworkBattery.Enabled = false;
                    netBat.NetworkBattery.Capacity = 0;
                    netBat.NetworkBattery.MaxChargeRate = 0;
                    netBat.NetworkBattery.SupplyRampRate = 0;
                    netBat.NetworkBattery.CurrentStorage = 0;
                }
            }
        }

        private void PostSync(NetworkBatteryPostSync ev)
        {
            foreach (var (netBat, bat) in EntityManager.EntityQuery<PowerNetworkBatteryComponent, BatteryComponent>())
            {
                bat.CurrentCharge = netBat.NetworkBattery.CurrentStorage;
            }

            foreach (var (netBat, cell) in EntityManager.EntityQuery<PowerNetworkBatteryComponent, PowerCellSlotComponent>())
            {
                if(_cellSystem.TryGetBatteryFromSlot(netBat.Owner, out var bat, cell))
                {
                    bat.CurrentCharge = netBat.NetworkBattery.CurrentStorage;
                }
            }
        }

        public override void Update(float frameTime)
        {
            foreach (var (comp, batt) in EntityManager.EntityQuery<BatterySelfRechargerComponent, BatteryComponent>())
            {
                if (!comp.AutoRecharge) continue;
                if (batt.IsFullyCharged) continue;
                batt.CurrentCharge += comp.AutoRechargeRate * frameTime;
            }
        }

        /// <summary>
        /// Gets the price for the power contained in an entity's battery.
        /// </summary>
        private void CalculateBatteryPrice(EntityUid uid, BatteryComponent component, ref PriceCalculationEvent args)
        {
            args.Price += component.CurrentCharge * component.PricePerJoule;
        }
    }
}
