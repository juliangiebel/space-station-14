using Content.Shared.Radiation.Events;
using Content.Shared.Tag;
using Robust.Shared.Map;

namespace Content.Server.Radiation.Systems;

public sealed class RadiationSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public void IrradiateRange(MapCoordinates coordinates, float range, float radsPerSecond, float time)
    {
        var lookUp = _lookup.GetEntitiesInRange(coordinates, range);
        IrradiateEntities(lookUp, radsPerSecond, time);
    }

    public void IrradiateRangeUnobstructed(MapCoordinates coordinates, float range, float radsPerSecond, float time)
    {
        var lookUp = _lookup.GetEntitiesInRange(coordinates, range, EntityLookupSystem.DefaultFlags | LookupFlags.Approximate);
        var filteredEntities = new HashSet<EntityUid>(lookUp.Count);

        foreach (var entityUid in lookUp)
        {
            if (_tagSystem.HasTag(entityUid, "RadiationReceiver")) filteredEntities.Add(entityUid);
        }

        IrradiateEntities(lookUp, radsPerSecond, time);
    }

    public void IrradiateEntities(IEnumerable<EntityUid> entities, float radsPerSecond, float time)
    {
        foreach (var uid in entities)
        {
            if (Deleted(uid))
                continue;

            IrradiateEntity(uid, radsPerSecond, time);
        }
    }

    public void IrradiateEntity(EntityUid uid, float radsPerSecond, float time)
    {
        var msg = new OnIrradiatedEvent(time, radsPerSecond);
        RaiseLocalEvent(uid, msg);
    }
}
