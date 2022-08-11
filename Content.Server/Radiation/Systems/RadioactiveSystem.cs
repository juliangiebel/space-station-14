using Content.Server.Radiation.Components;

namespace Content.Server.Radiation.Systems;

public sealed class RadioactiveSystem : EntitySystem
{
    [Dependency] private readonly RadiationSystem _radiationSystem = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var component in EntityManager.EntityQuery<RadioactiveComponent>())
        {
            _radiationSystem.IrradiateRangeUnobstructed(Transform(component.Owner).MapPosition, component.Range, component.Strength, frameTime);
        }
    }
}
