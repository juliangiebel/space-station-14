
using Content.Server.Atmos;
using Content.Shared.Atmos;
using Content.Shared.Disposal.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;
using System;

namespace Content.Server.Disposal.Unit.Components
{
    [RegisterComponent]
    public class DisposalInserterComponent : SharedDisposalInserterComponent
    {
        /// <summary>
        ///     The delay for an entity trying to move out of this inserter.
        /// </summary>
        [ViewVariables]
        [DataField("exitAttemptDelay")]
        public TimeSpan ExitAttemptDelay = TimeSpan.FromSeconds(0.5);

        /// <summary>
        ///     The delay for an entity trying to move into this inserter.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("entryDelay")]
        public float EntryDelay = 0.5f;

        /// <summary>
        ///     Delay from trying to shove someone else into disposals.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("draggedEntryDelay")]
        public float DraggedEntryDelay = 0.5f;

        /// <summary>
        /// The time it takes until the inserter engages itself after an entity was inserted
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("engageTime")]
        public TimeSpan AutomaticEngageTime = TimeSpan.FromSeconds(30);

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("flushDelay")]
        public TimeSpan FlushDelay = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     The engage pressure of this inserter.
        ///     Prevents it from flushing if the air pressure inside it is not equal to or bigger than this.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("engagePressure")]
        public float EngagePressure = 101.0f;

        /// <summary>
        /// The rate at wich the the inserter fills up with the surrounding atmosphere
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("pumpRate")]
        public int PumpRate = 1;

        [ViewVariables]
        public Container Container = default!;

        [ViewVariables]
        [DataField("air")]
        public GasMixture Air = new GasMixture(Atmospherics.CellVolume / 2);

        [ViewVariables]
        public bool Engaged;

        public TimeSpan LastExitAttempt;

        public bool Pressurized = false;
        //public bool ManualFlushReady = false;
        public bool QueuedFlush = false;
        public bool Flush = false;

        #region Verbs
        /*[Verb]
        private sealed class SelfInsertVerb : Verb<EntityInputComponent>
        {
            protected override void GetData(IEntity user, EntityInputComponent component, VerbData data)
            {
                data.Visibility = VerbVisibility.Invisible;

                if (!ActionBlockerSystem.CanInteract(user) ||
                    component.ContainedEntities.Contains(user))
                {
                    return;
                }

                data.Visibility = VerbVisibility.Visible;
                data.Text = Loc.GetString("Jump inside");
            }

            protected override void Activate(IEntity user, EntityInputComponent component)
            {
                _ = component.TryInsert(user, user);
            }
        }

        [Verb]
        private sealed class EjectContentsVerb : Verb<EntityInputComponent>
        {
            protected override void GetData(IEntity user, EntityInputComponent component, VerbData data)
            {
                data.Visibility = VerbVisibility.Invisible;

                if (!ActionBlockerSystem.CanInteract(user) ||
                    component.ContainedEntities.Contains(user))
                {
                    return;
                }

                data.Visibility = VerbVisibility.Visible;
                data.Text = Loc.GetString("Eject contents");
            }

            protected override void Activate(IEntity user, EntityInputComponent component)
            {
                component.TryEjectContents();
            }
        }*/
        #endregion
    }
}
