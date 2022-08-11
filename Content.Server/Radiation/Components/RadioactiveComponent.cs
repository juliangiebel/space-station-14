using Content.Server.Radiation.Systems;

namespace Content.Server.Radiation.Components;

[Friend(typeof(RadioactiveSystem))]
[RegisterComponent]
public sealed class RadioactiveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("strength")]
    public float Strength = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("range")]
    public float Range = 10f;
}
