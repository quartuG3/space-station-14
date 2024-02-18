using Content.Shared.Examine;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Destructible;
using Content.Shared.Coordinates.Helpers;
using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Content.Shared.PowerCell.Components;
using Content.Shared.Interaction.Events;
using Robust.Shared.Timing;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Utility;


namespace Content.Server.Holosign
{
    public sealed class HolosignSystem : EntitySystem
    {
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly PowerCellSystem _powerCell = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<HolosignProjectorComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<HolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
            SubscribeLocalEvent<HolosignProjectorComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<HolosignProjectorComponent, ComponentRemove>(OnRemove);
            SubscribeLocalEvent<HolosignProjectorComponent, GetVerbsEvent<Verb>>(AddClearVerb);
            SubscribeLocalEvent<HolosignBarrierComponent, ComponentRemove>(OnChildRemove);
            SubscribeLocalEvent<HolosignBarrierComponent, DestructionEventArgs>(OnChildDestroyed);
        }

        private void OnAfterInteract(EntityUid uid, HolosignProjectorComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach)
            {
                _popupSystem.PopupEntity(Loc.GetString("holoprojector-component-player-cannot-reach-message"), args.User, args.User);
                return;
            }

            // Not a holosign or holosign what put on map by mapper.
            if((args.Target == null) || !EntityManager.TryGetComponent(args.Target.Value, out HolosignBarrierComponent? holosigncomponent))
            {
                return;
            }

            if(!component.Childs.Contains(args.Target.Value))
            {
                _popupSystem.PopupEntity(Loc.GetString("holoprojector-component-player-not-a-child"), args.User, args.User);
                return;
            }

            // Would be fun see removing holoprojection without holosign component.
            _entManager.DeleteEntity(args.Target.Value);
            _popupSystem.PopupEntity(Loc.GetString("holoprojector-component-holosign-removed"), args.User, args.User);

            args.Handled = true;
        }

        private void OnExamine(EntityUid uid, HolosignProjectorComponent component, ExaminedEvent args)
        {
            var childs = component.Childs.Count;
            var max = component.MaxSigns;
            args.PushMarkup(Loc.GetString("holoprojector-component-examine", ("amount", childs), ("max", max)));
        }

        private void ClearHolosignsVerb(EntityUid uid, HolosignProjectorComponent component, EntityUid player)
        {
            _popupSystem.PopupEntity(Loc.GetString("holoprojector-component-holosigns-cleared"), player, player);

            // Should be cleared by OnChildRemove event?
            foreach (var child in component.Childs.ToArray())
            {
                if(!_entManager.Deleted(child))
                {
                    _entManager.DeleteEntity(child);
                }
            }
        }

        private void OnRemove(EntityUid uid, HolosignProjectorComponent component, ComponentRemove args)
        {
            foreach (var child in component.Childs.ToArray())
            {
                if(!_entManager.Deleted(child))
                {
                    _entManager.DeleteEntity(child);
                }
            }
        }

        private void AddClearVerb(EntityUid uid, HolosignProjectorComponent component, GetVerbsEvent<Verb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
                return;

            if(component.Childs.Count > 0)
            {
                Verb clear = new ()
                {
             	    Act = () => ClearHolosignsVerb(uid, component, args.User),
                    Text = Loc.GetString("holoprojector-component-verb-clear"),
                    Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/rotate_cw.svg.192dpi.png")),
                    Priority = 1,
                    CloseMenu = true, // allow for easy double rotations.
                };
                args.Verbs.Add(clear);
            }
        }

        private void OnChildRemove(EntityUid uid, HolosignBarrierComponent component, ComponentRemove args)
        {
            if(_entManager.Deleted(component.Holoprojector))
                return;

            // Holoprojector without Holoprojector component. BRUH
            if(EntityManager.TryGetComponent(component.Holoprojector, out HolosignProjectorComponent? holoprojector))
            {
                holoprojector.Childs.Remove(uid);
            }
        }

        private void OnChildDestroyed(EntityUid uid, HolosignBarrierComponent component, DestructionEventArgs args)
        {
            if(_entManager.Deleted(component.Holoprojector))
                return;

            // Holoprojector without Holoprojector component. BRUH
            if(EntityManager.TryGetComponent(component.Holoprojector, out HolosignProjectorComponent? holoprojector))
            {
                holoprojector.Childs.Remove(uid);
            }
        }
        private void OnBeforeInteract(EntityUid uid, HolosignProjectorComponent component, BeforeRangedInteractEvent args)
        {

            if (args.Handled
                || !args.CanReach // prevent placing out of range
                || HasComp<StorageComponent>(args.Target) // if it's a storage component like a bag, we ignore usage so it can be stored
               )
                return;

            if(component.Childs.Count >= component.MaxSigns)
            {
                _popupSystem.PopupEntity(Loc.GetString("holoprojector-component-holosigns-limit"), args.User, args.User);
            }

            // places the holographic sign at the click location, snapped to grid.
            // overlapping of the same holo on one tile remains allowed to allow holofan refreshes
            var holoUid = EntityManager.SpawnEntity(component.SignProto, args.ClickLocation.SnapToGrid(EntityManager));
            var xform = Transform(holoUid);
            if (!xform.Anchored)
                _transform.AnchorEntity(holoUid, xform); // anchor to prevent any tempering with (don't know what could even interact with it)

            var holoComp = _entManager.AddComponent<HolosignBarrierComponent>(holoUid);
            holoComp.Holoprojector = uid;
            component.Childs.Add(holoUid);

            args.Handled = true;
        }
    }
}
