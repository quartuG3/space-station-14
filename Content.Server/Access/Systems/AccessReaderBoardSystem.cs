using System.Linq;
using Content.Shared.Interaction.Events;
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
        SubscribeLocalEvent<AccessStorageComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, AccessStorageComponent component, UseInHandEvent args)
    {
        List<string> accessList = new List<string>();

        if (component.AccessLists.Count > 0)
        {
            foreach (HashSet<string> hashSet in component.AccessLists)
            {
                foreach (string hash in hashSet.ToArray())
                {
                    accessList.Add(hash);
                }
            }
        }
        AccessStorageBoundUserInterfaceState newState = new(accessList.ToArray(), component.DenyTags.ToArray());
        _userInterfaceSystem.TrySetUiState(uid, AccessStorageUiKey.Key, newState);
    }
}
