using Content.Shared.GameObjects.Components.Movement;
using Content.Shared.GameObjects.Components.Movement.Content.Server.GameObjects.Components.Movement;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Client.GameObjects.Components.Movement
{
    [RegisterComponent]
    [ComponentReference(typeof(IShipMovementComponent))]
    class ShipControllerComponent : SharedShipControllerComponent
    {
    }
}
