using Content.Server.Disposal.Unit.Components;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Disposal.Unit.EntitySystems
{
    public class DisposalInserterSystem : EntitySystem
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
            SubscribeLocalEvent<DisposalInserterComponent, DropAttemptEvent>(OnDragDropAttempt);
            SubscribeLocalEvent<DisposalInserterComponent, EntInsertedIntoContainerMessage>(OnContainerInserted);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        /*public void Engage()
        {

        }*/

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

        }

        private void OnThrowCollide(EntityUid uid, DisposalInserterComponent component, ThrowCollideEvent args)
        {

        }

        private void OnDragDrop(EntityUid uid, DisposalInserterComponent component, DragDropRequestEvent args)
        {

        }

        private void OnDragDropAttempt(EntityUid uid, DisposalInserterComponent component, DropAttemptEvent args)
        {

        }

        private void OnContainerInserted(EntityUid uid, DisposalInserterComponent component, EntInsertedIntoContainerMessage args)
        {
            //Start autoflush timer, change state, change apearance
        }
    }
}
