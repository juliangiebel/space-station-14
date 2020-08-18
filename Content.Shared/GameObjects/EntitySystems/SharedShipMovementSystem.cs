#nullable enable
using System.Diagnostics.CodeAnalysis;
using Content.Shared.GameObjects.Components.Items;
using Content.Shared.GameObjects.Components.Movement;
using Content.Shared.Physics;
using Content.Shared.Physics.Pull;
using Robust.Shared.GameObjects.Components;
using Robust.Shared.GameObjects.Systems;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.GameObjects.Components;
using Robust.Shared.Interfaces.Physics;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Players;

namespace Content.Shared.GameObjects.EntitySystems
{
    public abstract class SharedShipMovementSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPhysicsManager _physicsManager = default!;
        //[Dependency] private readonly IConfigurationManager _configurationManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            var moveUpCmdHandler = new MoverDirInputCmdHandler(Direction.North);
            var moveLeftCmdHandler = new MoverDirInputCmdHandler(Direction.West);
            var moveRightCmdHandler = new MoverDirInputCmdHandler(Direction.East);
            var moveDownCmdHandler = new MoverDirInputCmdHandler(Direction.South);

            CommandBinds.Builder
                .Bind(EngineKeyFunctions.MoveUp, moveUpCmdHandler)
                .Bind(EngineKeyFunctions.MoveLeft, moveLeftCmdHandler)
                .Bind(EngineKeyFunctions.MoveRight, moveRightCmdHandler)
                .Bind(EngineKeyFunctions.MoveDown, moveDownCmdHandler)
                .Register<SharedShipMovementSystem>();

            //_configurationManager.RegisterCVar("game.shipmovement", true, CVar.ARCHIVE);
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            CommandBinds.Unregister<SharedMoverSystem>();
            base.Shutdown();
        }

        protected void UpdateKinematics(ITransformComponent transform, IShipMovementComponent shipMover, ICollidableComponent collidable)
        {
            collidable.EnsureController<MoverController>();

            //var weightless = !transform.Owner.HasComponent<MovementIgnoreGravityComponent>() &&
            //                 _physicsManager.IsWeightless(transform.GridPosition);

            //if (weightless)
            //{
            //    // No gravity: is our entity touching anything?
            //    var touching = IsAroundCollider(transform, mover, collidable);

            //    if (!touching)
            //    {
            //        return;
            //    }
            //}

            // TODO: movement check.
            var flyDir = shipMover.VelocityDir;
            if (flyDir.LengthSquared < 0.001 || !ActionBlockerSystem.CanMove(shipMover.Owner))
            {
                if (collidable.TryGetController(out MoverController controller))
                {
                    controller.StopMoving();
                }
            }
            else
            {
                //if (weightless)
                //{
                //    if (collidable.TryGetController(out MoverController controller))
                //    {
                //        controller.Push(flyDir, shipMover.CurrentPushSpeed);
                //    }

                //    transform.LocalRotation = flyDir.GetDir().ToAngle();
                //    return;
                //}

                var total = flyDir * shipMover.CurrentFlySpeed;

                {
                    if (collidable.TryGetController(out MoverController controller))
                    {
                        controller.Move(total, 1);
                    }
                }

                transform.LocalRotation = total.GetDir().ToAngle();

                HandleEngineSounds(shipMover);
            }
        }

        protected virtual void HandleEngineSounds(IShipMovementComponent shipMover)
        {

        }

        private bool IsAroundCollider(ITransformComponent transform, IShipMovementComponent shipMover,
            ICollidableComponent collider)
        {
            foreach (var entity in _entityManager.GetEntitiesInRange(transform.Owner, shipMover.CollisionCheckRange, true))
            {
                if (entity == transform.Owner)
                {
                    continue; // Don't try to push off of yourself!
                }

                if (!entity.TryGetComponent<ICollidableComponent>(out var otherCollider))
                {
                    continue;
                }

                // Don't count pulled entities
                if (otherCollider.HasController<PullController>())
                {
                    continue;
                }

                // TODO: Item check.
                var touching = ((collider.CollisionMask & otherCollider.CollisionLayer) != 0x0
                                || (otherCollider.CollisionMask & collider.CollisionLayer) != 0x0) // Ensure collision
                               && !entity.HasComponent<IItemComponent>(); // This can't be an item

                if (touching)
                {
                    return true;
                }
            }

            return false;
        }


        private static void HandleDirChange(ICommonSession? session, Direction dir, ushort subTick, bool state)
        {
            if (!TryGetAttachedComponent<IShipMovementComponent>(session, out var shipMover))
                return;

            var owner = session?.AttachedEntity;

            if (owner != null)
            {
                foreach (var comp in owner.GetAllComponents<IRelayMoveInput>())
                {
                    comp.MoveInputPressed(session);
                }
            }

            shipMover.SetVelocityDirection(dir, subTick, state);
        }

        private static void HandleBoostChange(ICommonSession? session, ushort subTick, bool boosting)
        {
            if (!TryGetAttachedComponent<IShipMovementComponent>(session, out var shipMover))
            {
                return;
            }

            shipMover.SetBoosting(subTick, boosting);
        }

        private static bool TryGetAttachedComponent<T>(ICommonSession? session, [MaybeNullWhen(false)] out T component)
            where T : IComponent
        {
            component = default;

            var ent = session?.AttachedEntity;

            if (ent == null || !ent.IsValid())
                return false;

            if (!ent.TryGetComponent(out T comp))
                return false;

            component = comp;
            return true;
        }

        private sealed class MoverDirInputCmdHandler : InputCmdHandler
        {
            private readonly Direction _dir;

            public MoverDirInputCmdHandler(Direction dir)
            {
                _dir = dir;
            }

            public override bool HandleCmdMessage(ICommonSession? session, InputCmdMessage message)
            {
                if (!(message is FullInputCmdMessage full))
                {
                    return false;
                }

                HandleDirChange(session, _dir, message.SubTick, full.State == BoundKeyState.Down);
                return false;
            }
        }

        private sealed class FlyInputCmdHandler : InputCmdHandler
        {
            public override bool HandleCmdMessage(ICommonSession? session, InputCmdMessage message)
            {
                if (!(message is FullInputCmdMessage full))
                {
                    return false;
                }

                //HandleBoostChange(session, full.SubTick, full.State == BoundKeyState.Down);
                return false;
            }
        }
    }
}
