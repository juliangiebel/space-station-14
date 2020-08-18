using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared.GameObjects.Components.Movement
{
#nullable enable
    using Shared.Physics;
    using Robust.Shared.GameObjects;
    using Robust.Shared.GameObjects.Components;
    using Robust.Shared.Interfaces.GameObjects;
    using Robust.Shared.Interfaces.Map;
    using Robust.Shared.IoC;
    using Robust.Shared.Log;
    using Robust.Shared.Map;
    using Robust.Shared.Maths;
    using Robust.Shared.ViewVariables;

    namespace Content.Server.GameObjects.Components.Movement
    {
        public class SharedShipControllerComponent : Component, IShipMovementComponent
        {
#pragma warning disable 649
            [Dependency] private readonly IMapManager _mapManager = default!;
            [Dependency] private readonly IEntityManager _entityManager = default!;
#pragma warning restore 649

            private bool _movingUp;
            private bool _movingDown;
            private bool _movingLeft;
            private bool _movingRight;

            public sealed override string Name => "ShipController";

            [ViewVariables(VVAccess.ReadWrite)]
            public float CurrentFlySpeed => 8;

            public bool Boosting => throw new System.NotImplementedException();

            public float CollisionCheckRange => throw new System.NotImplementedException();

            Vector2 IShipMovementComponent.VelocityDir => Vector2.Zero;

            public float EngineSoundDistance { get; set; }

            public GridCoordinates LastPosition { get; set; }

            public void SetBoosting(ushort subTick, bool flying)
            {
                throw new System.NotImplementedException();
            }

            public void SetVelocityDirection(Direction direction, ushort subTick, bool enabled)
            {
                var gridId = Owner.Transform.GridID;

                if (_mapManager.TryGetGrid(gridId, out var grid) &&
                    _entityManager.TryGetEntity(grid.GridEntityId, out var gridEntity))
                {
                    if (!gridEntity.TryGetComponent(out ICollidableComponent collidable))
                    {
                        collidable = gridEntity.AddComponent<CollidableComponent>();
                        collidable.Mass = 1;
                        collidable.CanCollide = true;
                        collidable.BodyType = Robust.Shared.Physics.BodyType.Dynamic;
                        collidable.PhysicsShapes.Add(new PhysShapeGrid(grid));
                    }

                    var controller = collidable.EnsureController<ShipController>();
                    controller.Move(CalcNewVelocity(direction, enabled), CurrentFlySpeed);
                }
            }

            private Vector2 CalcNewVelocity(Direction direction, bool enabled)
            {
                switch (direction)
                {
                    case Direction.East:
                        _movingRight = enabled;
                        break;
                    case Direction.North:
                        _movingUp = enabled;
                        break;
                    case Direction.West:
                        _movingLeft = enabled;
                        break;
                    case Direction.South:
                        _movingDown = enabled;
                        break;
                }

                // key directions are in screen coordinates
                // _moveDir is in world coordinates
                // if the camera is moved, this needs to be changed

                var x = 0;
                x -= _movingLeft ? 1 : 0;
                x += _movingRight ? 1 : 0;

                var y = 0;
                y -= _movingDown ? 1 : 0;
                y += _movingUp ? 1 : 0;

                var result = new Vector2(x, y);

                // can't normalize zero length vector
                if (result.LengthSquared > 1.0e-6)
                {
                    result = result.Normalized;
                }

                return result;
            }
        }
    }

}
