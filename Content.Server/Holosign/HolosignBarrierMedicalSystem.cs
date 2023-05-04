using Content.Server.Construction;
using Content.Server.Popups;
using Content.Shared.Examine;
using Content.Shared.Holosign;
using Content.Shared.Interaction;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared.Interaction;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;

namespace Content.Server.Holosign
{
    public sealed class HolosignBarrierMedicalSystem : EntitySystem
    {
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HolosignBarrierMedicalComponent, InteractHandEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, HolosignBarrierMedicalComponent component, InteractHandEvent args)
        {
            if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target))
                return;

            if (!_entManager.TryGetComponent(args.User, out ActorComponent? actor))
                return;
        }

    }
}
