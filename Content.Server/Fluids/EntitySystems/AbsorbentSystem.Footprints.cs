using System.Linq;
using Content.Shared.Chemistry.Components;
using Content.Shared.Fluids;
using Content.Shared.FootPrint;
using Content.Shared.Timing;


namespace Content.Server.Fluids.EntitySystems;


// Floof-specific
public sealed partial class AbsorbentSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    /// <summary>
    ///     Tries to clean a number of footprints in a range determined by the component. Returns the number of cleaned footprints.
    /// </summary>
    private int TryCleanNearbyFootprints(EntityUid user, EntityUid used, EntityUid target, AbsorbentComponent component,  Entity<SolutionComponent> absorbentSoln)
    {
        var footprintQuery = GetEntityQuery<FootPrintComponent>();
        var targetCoords = Transform(target).Coordinates;
        var entities = _lookup.GetEntitiesInRange(targetCoords, component.FootprintCleaningRange, LookupFlags.Uncontained);

        // Take up to [MaxCleanedFootprints] footprints closest to the target
        var cleaned = entities.AsEnumerable()
            .Where(footprintQuery.HasComp)
            .Select(uid => (uid, dst: Transform(uid).Coordinates.TryDistance(EntityManager, _transform, targetCoords, out var dst) ? dst : 0f))
            .Where(ent => ent.dst > 0f)
            .OrderBy(ent => ent.dst)
            .Select(ent => (ent.uid, comp: footprintQuery.GetComponent(ent.uid)));

        // And try to interact with each one of them, ignoring useDelay
        var processed = 0;
        foreach (var (uid, footprintComp) in cleaned)
        {
            if (TryPuddleInteract(user, used, uid, component, useDelay: null, absorbentSoln))
                processed++;

            if (processed >= component.MaxCleanedFootprints)
                break;
        }

        return processed;
    }
}
