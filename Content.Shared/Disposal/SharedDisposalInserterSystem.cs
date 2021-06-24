using Content.Shared.Body.Components;
using Content.Shared.Disposal.Components;
using Content.Shared.DragDrop;
using Content.Shared.Item;
using Content.Shared.MobState;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;

namespace Content.Shared.Disposal
{
    public class SharedDisposalInserterSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SharedDisposalInserterComponent, DropAttemptEvent>(OnDragDropAttempt);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        public virtual bool CanInsert(IEntity entity, Container container = default!)
        {
            // TODO: Probably just need a disposable tag.
            if (!entity.TryGetComponent(out SharedItemComponent? storable) &&
                !entity.HasComponent<SharedBodyComponent>())
            {
                return false;
            }


            if (!entity.TryGetComponent(out IPhysBody? physics) ||
                physics.BodyType != BodyType.Static || !physics.CanCollide && storable == null)
            {
                if (!(entity.TryGetComponent(out IMobStateComponent? damageState) && damageState.IsDead()))
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDragDropAttempt(EntityUid uid, SharedDisposalInserterComponent component, DropAttemptEvent args)
        {
            if (!CanInsert(args.Entity))
            {
                args.Cancel();
            }
        }
    }
}
