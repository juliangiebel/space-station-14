#nullable enable
using Content.Server.GameObjects.Components.Buckle;
using Content.Server.GameObjects.Components.Mobs;
using Content.Shared.GameObjects.Components.Mobs;
using Content.Shared.GameObjects.Components.Movement;
using Content.Shared.GameObjects.Components.Movement.Content.Server.GameObjects.Components.Movement;
using Content.Shared.GameObjects.Components.Strap;
using Content.Shared.Physics;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Components;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.Map;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;

namespace Content.Server.GameObjects.Components.Movement
{
    [RegisterComponent]
    [ComponentReference(typeof(IShipMovementComponent))]
    internal class ShipControllerComponent : SharedShipControllerComponent
    {

        /// <summary>
        ///     The icon to be displayed when piloting from this chair.
        /// </summary>
        private string _pilotingIcon = default!;

        /// <summary>
        ///     The entity that's currently controlling this component.
        ///     Changed from <see cref="SetController"/> and <see cref="RemoveController"/>
        /// </summary>
        private IEntity? _controller;

        /// <summary>
        ///     Changes the entity currently controlling this shuttle controller
        /// </summary>
        /// <param name="entity">The entity to set</param>
        private void SetController(IEntity entity)
        {
            if (_controller != null ||
                !entity.TryGetComponent(out MindComponent mind) ||
                mind.Mind == null ||
                !Owner.TryGetComponent(out ServerStatusEffectsComponent status))
            {
                return;
            }

            mind.Mind.Visit(Owner);
            _controller = entity;

            status.ChangeStatusEffectIcon(StatusEffect.Piloting, _pilotingIcon);
        }

        /// <summary>
        ///     Removes the current controller
        /// </summary>
        /// <param name="entity">The entity to remove, or null to force the removal of any current controller</param>
        public void RemoveController(IEntity? entity = null)
        {
            if (_controller == null)
            {
                return;
            }

            // If we are not forcing a controller removal and the entity is not the current controller
            if (entity != null && entity != _controller)
            {
                return;
            }

            UpdateRemovedEntity(entity ?? _controller);

            _controller = null;
        }

        /// <summary>
        ///     Updates the state of an entity that is no longer controlling this shuttle controller.
        ///     Called from <see cref="RemoveController"/>
        /// </summary>
        /// <param name="entity">The entity to update</param>
        private void UpdateRemovedEntity(IEntity entity)
        {
            if (Owner.TryGetComponent(out ServerStatusEffectsComponent status))
            {
                status.RemoveStatusEffect(StatusEffect.Piloting);
            }

            if (entity.TryGetComponent(out MindComponent mind))
            {
                mind.Mind?.UnVisit();
            }

            if (entity.TryGetComponent(out BuckleComponent buckle))
            {
                buckle.TryUnbuckle(entity, true);
            }
        }

        private void BuckleChanged(IEntity entity, in bool buckled)
        {
            Logger.DebugS("ship", $"Pilot={entity.Name}, buckled={buckled}");

            if (buckled)
            {
                SetController(entity);
            }
            else
            {
                RemoveController(entity);
            }
        }

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _pilotingIcon, "pilotingIcon", "/Textures/Interface/StatusEffects/Buckle/buckled.png");
        }

        public override void Initialize()
        {
            base.Initialize();
            Owner.EnsureComponent<ServerStatusEffectsComponent>();
        }

        /// <inheritdoc />
        public override void HandleMessage(ComponentMessage message, IComponent? component)
        {
            base.HandleMessage(message, component);

            switch (message)
            {
                case StrapChangeMessage strap:
                    BuckleChanged(strap.Entity, strap.Buckled);
                    break;
            }
        }
    }
}
