using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body;
using Content.Server.Disposal.Unit.Components;
using Content.Server.DoAfter;
using Content.Server.Hands.Components;
using Content.Server.Items;
using Content.Server.MobState.States;
using Content.Shared.Disposal;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.MobState;
using Content.Shared.Movement;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Threading.Tasks;

namespace Content.Server.Disposal.Unit.EntitySystems
{
    public class DisposalInserterSystem : SharedDisposalInserterSystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] readonly IRobustRandom _random = default!;
        [Dependency] private readonly IContainerManager _containerManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DisposalInserterComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DisposalInserterComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<DisposalInserterComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<DisposalInserterComponent, ThrowCollideEvent>(OnThrowCollide);
            SubscribeLocalEvent<DisposalInserterComponent, DragDropRequestEvent>(OnDragDrop);
            SubscribeLocalEvent<DisposalInserterComponent, EntInsertedIntoContainerMessage>(OnContainerInserted);
            SubscribeLocalEvent<DisposalInserterComponent, MovementAttemptEvent>(OnMovementAttempt);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        public override bool CanInsert(IEntity entity, Container container)
        {
            return base.CanInsert(entity) && container.CanInsert(entity);
        }

        public async Task<bool> TryInsert(DisposalInserterComponent component, IEntity entity, IEntity? user = default, bool check = true)
        {
            if (check && !CanInsert(entity, component.Container))
                return false;

            var delay = user == entity ? component.EntryDelay : component.DraggedEntryDelay;

            if (user != null && delay > 0.0f)
            {
                var doAfterSystem = Get<DoAfterSystem>();

                // Can't check if our target AND disposals moves currently so we'll just check target.
                // if you really want to check if disposals moves then add a predicate.
                var doAfterArgs = new DoAfterEventArgs(user, delay, default, entity)
                {
                    BreakOnDamage = true,
                    BreakOnStun = true,
                    BreakOnTargetMove = true,
                    BreakOnUserMove = true,
                    NeedHand = false,
                };

                var result = await doAfterSystem.DoAfter(doAfterArgs);

                if (result == DoAfterStatus.Cancelled)
                    return false;
            }

            if (!component.Container.Insert(entity))
                return false;

            return true;
        }

        private void OnStartup(EntityUid uid, DisposalInserterComponent component, ComponentStartup args)
        {
            if (component.Container == default!)
            {
                component.Container =  ContainerHelpers.EnsureContainer<Container>(EntityManager.GetEntity(uid), component.Name);
            }
        }

        private void OnShutdown(EntityUid uid, DisposalInserterComponent component, ComponentShutdown args)
        {
            ContainerHelpers.EmptyContainer(component.Container, true);
        }

        private void OnInteractUsing(EntityUid uid, DisposalInserterComponent component, InteractUsingEvent args)
        {
            if (!CanInsert(args.Used, component.Container))
                return;

            _ = TryInsert(component, args.Used, args.User, false);
            args.Handled = true;
        }

        private void OnThrowCollide(EntityUid uid, DisposalInserterComponent component, ThrowCollideEvent args)
        {
            if (!CanInsert(args.Thrown, component.Container))
                return;

            _ = TryInsert(component, args.Thrown, check: false);
            args.Handled = true;
        }

        private void OnDragDrop(EntityUid uid, DisposalInserterComponent component, DragDropRequestEvent args)
        {
            _ = TryInsert(component, EntityManager.GetEntity(args.Dropped));
        }

        private void OnContainerInserted(EntityUid uid, DisposalInserterComponent component, EntInsertedIntoContainerMessage args)
        {
            //Start autoflush timer, change state, change apearance
        }

        private void OnMovementAttempt(EntityUid uid, DisposalInserterComponent component, MovementAttemptEvent args)
        {
            if (!args.Entity.TryGetComponent(out HandsComponent? hands) || hands.Count == 0 || _gameTiming.CurTime < component.LastExitAttempt + component.ExitAttemptDelay)
            {
                return;
            }

            component.LastExitAttempt = _gameTiming.CurTime;
            ContainerHelpers.TryRemoveFromContainer(args.Entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the inserter was able to increase its pressure</returns>
        private bool Pressurize(DisposalInserterComponent component, IEntity entity)
        {
            if (!entity.Transform.Coordinates.TryGetTileAtmosphere(out var tileAtmos) || tileAtmos.Air == null || tileAtmos.Air.Temperature <= 0)
            {
                return false;
            }

            component.Pressurized = false;

            var tileAir = tileAtmos.Air;
            var volumeRatio = System.Math.Clamp(component.PumpRate / tileAir.Volume, 0, 1);

            var atmosSystem = Get<AtmosphereSystem>();

            atmosSystem.Merge(component.Air, tileAir.RemoveRatio(volumeRatio));

            atmosSystem.GetGridAtmosphere(entity.Transform.GridID)?.Invalidate(tileAtmos.GridIndices);

            //SendMessage(new PressureChangedMessage(Air.Pressure, _engagePressure));

            return true;
        }
    }
}
