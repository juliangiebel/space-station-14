﻿#nullable enable
using Content.Server.GameObjects.Components;
using Content.Server.GameObjects.Components.Movement;
using Content.Shared.Audio;
using Content.Shared.GameObjects.Components.Movement;
using Content.Shared.GameObjects.EntitySystems;
using Content.Shared.Maps;
using Content.Shared.Physics;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.GameObjects.EntitySystems;
using Robust.Server.Interfaces.Timing;
using Robust.Shared.GameObjects.Components;
using Robust.Shared.GameObjects.Components.Transform;
using Robust.Shared.Interfaces.Map;
using Robust.Shared.Interfaces.Random;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.GameObjects.EntitySystems
{
    [UsedImplicitly]
    internal class ShipMovementSystem : SharedShipMovementSystem
    {
#pragma warning disable 649
        [Dependency] private readonly IPauseManager _pauseManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
#pragma warning restore 649

        private AudioSystem _audioSystem = default!;

        private const float EngineSoundMoveDistanceBoosting = 2;
        private const float EngineSoundMoveDistanceFlying = 1.5f;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlayerAttachSystemMessage>(PlayerAttached);
            SubscribeLocalEvent<PlayerDetachedSystemMessage>(PlayerDetached);

            _audioSystem = EntitySystemManager.GetEntitySystem<AudioSystem>();

            UpdatesBefore.Add(typeof(PhysicsSystem));
        }

        public override void Update(float frameTime)
        {
            foreach (var (moverComponent, collidableComponent) in EntityManager.ComponentManager.EntityQuery<IShipMovementComponent, ICollidableComponent>())
            {
                var entity = moverComponent.Owner;
                if (_pauseManager.IsEntityPaused(entity))
                    continue;

                UpdateKinematics(entity.Transform, moverComponent, collidableComponent);
            }
        }

        private static void PlayerAttached(PlayerAttachSystemMessage ev)
        {
            if (!ev.Entity.HasComponent<IMoverComponent>())
            {
                ev.Entity.AddComponent<PlayerInputMoverComponent>();
            }
        }

        private static void PlayerDetached(PlayerDetachedSystemMessage ev)
        {
            if (ev.Entity.HasComponent<PlayerInputMoverComponent>())
            {
                ev.Entity.RemoveComponent<PlayerInputMoverComponent>();
            }

            if (ev.Entity.TryGetComponent(out ICollidableComponent physics) &&
                physics.TryGetController(out MoverController controller))
            {
                controller.StopMoving();
            }
        }

        protected override void HandleEngineSounds(IShipMovementComponent mover)
        {
            //var transform = mover.Owner.Transform;
            //// Handle footsteps.
            //if (_mapManager.GridExists(mover.LastPosition.GridID))
            //{
            //    // Can happen when teleporting between grids.
            //    var distance = transform.GridPosition.Distance(_mapManager, mover.LastPosition);
            //    mover.StepSoundDistance += distance;
            //}

            //mover.LastPosition = transform.GridPosition;
            //float distanceNeeded;
            //if (mover.Sprinting)
            //{
            //    distanceNeeded = EngineSoundMoveDistanceBoosting;
            //}
            //else
            //{
            //    distanceNeeded = EngineSoundMoveDistanceFlying;
            //}

            //if (mover.StepSoundDistance > distanceNeeded)
            //{
            //    mover.StepSoundDistance = 0;

            //    if (!mover.Owner.HasComponent<FootstepSoundComponent>())
            //    {
            //        return;
            //    }

            //    if (mover.Owner.TryGetComponent<InventoryComponent>(out var inventory)
            //        && inventory.TryGetSlotItem<ItemComponent>(EquipmentSlotDefines.Slots.SHOES, out var item)
            //        && item.Owner.TryGetComponent<FootstepModifierComponent>(out var modifier))
            //    {
            //        modifier.PlayFootstep();
            //    }
            //    else
            //    {
            //        PlayFootstepSound(transform.GridPosition);
            //    }
            //}
        }

        private void PlayFootstepSound(GridCoordinates coordinates)
        {
            // Step one: figure out sound collection prototype.
            var grid = _mapManager.GetGrid(coordinates.GridID);
            var tile = grid.GetTileRef(coordinates);

            // If the coordinates have a catwalk, it's always catwalk.
            string soundCollectionName;
            var catwalk = false;
            foreach (var maybeCatwalk in grid.GetSnapGridCell(tile.GridIndices, SnapGridOffset.Center))
            {
                if (maybeCatwalk.Owner.HasComponent<CatwalkComponent>())
                {
                    catwalk = true;
                    break;
                }
            }

            if (catwalk)
            {
                // Catwalk overrides tile sound.s
                soundCollectionName = "footstep_catwalk";
            }
            else
            {
                // Walking on a tile.
                var def = (ContentTileDefinition) _tileDefinitionManager[tile.Tile.TypeId];
                if (def.FootstepSounds == null)
                {
                    // Nothing to play, oh well.
                    return;
                }

                soundCollectionName = def.FootstepSounds;
            }

            // Ok well we know the position of the
            try
            {
                var soundCollection = _prototypeManager.Index<SoundCollectionPrototype>(soundCollectionName);
                var file = _robustRandom.Pick(soundCollection.PickFiles);
                _audioSystem.PlayAtCoords(file, coordinates);
            }
            catch (UnknownPrototypeException)
            {
                // Shouldn't crash over a sound
                Logger.ErrorS("sound", $"Unable to find sound collection for {soundCollectionName}");
            }
        }
    }
}
