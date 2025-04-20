using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Map;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    protected override void Cycle(EntityUid uid, BallisticAmmoProviderComponent component, MapCoordinates coordinates, EntityUid? user)
    {
        EntityUid? ent = null;
        EntityUid? ejecting = null;

        // TODO: Combine with TakeAmmo
        if (component.Entities.Count > 0)
        {
            var existing = component.Entities[^1];
            component.Entities.RemoveAt(component.Entities.Count - 1);

            Containers.Remove(existing, component.Container);
            EnsureShootable(existing);
            ejecting = existing;
        }
        else if (component.UnspawnedCount > 0)
        {
            component.UnspawnedCount--;
            ent = Spawn(component.Proto, coordinates);
            EnsureShootable(ent.Value);
            ejecting = ent.Value;
        }

        if (ent != null)
            EjectCartridge(ent.Value);

        if (component.PutCycledIntoHand && ejecting != null && user != null)
            _handsSystem.PickupOrDrop(user.Value, ejecting.Value);

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(uid, ref cycledEvent);
    }
}
