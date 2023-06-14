using Content.Shared.Holosign;
using Robust.Shared.Physics.Events;
using Robust.Client.GameObjects;

namespace Content.Client.Holosign;

public sealed class HolosignBarrierMedicalSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolosignBarrierMedicalComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HolosignBarrierMedicalComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnCollide(EntityUid uid, HolosignBarrierMedicalComponent component, ref StartCollideEvent args)
    {
        var otherEnt = args.OtherEntity;

        if (!_entManager.TryGetComponent<SpriteComponent?>(uid, out var sprite))
            return;

        sprite.LayerSetState(0, "deny");
    }

    private void OnEndCollide(EntityUid uid, HolosignBarrierMedicalComponent component, ref EndCollideEvent args)
    {
     	var otherEnt = args.OtherEntity;

        if (!_entManager.TryGetComponent<SpriteComponent?>(uid, out var sprite))
            return;

        sprite.LayerSetState(0, "icon");
    }
}
