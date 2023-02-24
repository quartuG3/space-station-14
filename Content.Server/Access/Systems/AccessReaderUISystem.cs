using System.Linq;
using Content.Shared.Interaction.Events;
using Content.Shared.Interaction;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;

using Robust.Server.GameObjects;

namespace Content.Server.Access.Systems;

    public sealed class AccessReaderUISystem : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AccessReaderComponent, UseInHandEvent>(OnUseInHand);
        }

        private void OnUseInHand(EntityUid uid, AccessReaderComponent component, UseInHandEvent args)
        {
            string [] access = {""};

            if(component.AccessLists.Count > 0)
                access =  component.AccessLists[0].ToArray<string>();

            AccessReaderBoundUserInterfaceState newState = new(access, component.InvertedAccess);
            _userInterfaceSystem.TrySetUiState(uid, AccessReaderUiKey.Key, newState);
        }
        
    }
