using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Shared.GameObjects.Components.Movement
{
    public interface IShipMovementComponent : IComponent
    {
        /// <summary>
        ///     Movement speed (m/s) that the ship flies.
        /// </summary>
        float CurrentFlySpeed { get; }

        /// <summary>
        ///     Is the ship boosting?
        /// </summary>
        bool Boosting { get; }

        /// <summary>
        ///     The range Entities are considered for collision.
        ///     TODO: Determine if this is needed.
        /// </summary>
        float CollisionCheckRange { get; }


        /// <summary>
        ///     Calculated linear velocity direction of the entity.
        /// </summary>
        Vector2 VelocityDir { get; }

        GridCoordinates LastPosition { get; set; }

        float EngineSoundDistance { get; set; }

        /// <summary>
        ///     Toggles one of the four cardinal directions. Each of the four directions are
        ///     composed into a single direction vector. Enabling
        ///     opposite directions will cancel each other out, resulting in no direction.
        /// </summary>
        /// <param name="direction">Direction to toggle.</param>
        /// <param name="subTick"></param>
        /// <param name="enabled">If the direction is active.</param>
        void SetVelocityDirection(Direction direction, ushort subTick, bool enabled);

        void SetBoosting(ushort subTick, bool flying);

    }
}
