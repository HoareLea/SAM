// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Core
{

    public class SAMObjectRelationCluster : SAMObjectRelationCluster<IJSAMObject>
    {
        /// <summary>
        /// Replaces one stored object with a clone of itself, or <b>throws</b>.
        ///
        /// <para><b>Why a declared deep clone may not fall back to sharing</b></para>
        /// <para>
        /// <c>Core.Query.Clone</c> resolves by reflection - an instance <c>Clone()</c>, else a
        /// single-argument constructor accepting the type, else a parameterless one - and returns
        /// <c>default</c> when it finds none. Constructors are not inherited, so a subclass of a type that
        /// has a copy constructor does not have one; that is easy to add and easy to forget.
        /// </para>
        /// <para>
        /// The loop that calls this used to hand the null straight to <c>AddObject</c>, which rejected it
        /// and left the ORIGINAL instance sitting in the dictionary the shallow base constructor had
        /// already filled. The caller had asked for a copy that owns its objects and was given one that
        /// silently did not own that object - so an in-place write through the "deep" model reached the
        /// model it was copied from, which is the exact defect deep cloning exists to prevent, restored for
        /// one type and invisible.
        /// </para>
        /// <para>
        /// A deep clone that cannot be delivered is therefore an error rather than a quiet downgrade. The
        /// fix when this throws is to give the named type the copy support its siblings have, not to catch
        /// it: see <c>ZoneSimulationResult</c> and the <c>TM5x</c> results for the shape.
        /// </para>
        /// <para>
        /// The clone is returned for the caller to store <b>in the original's own slot</b> (see
        /// <c>RelationCluster.ReplaceObjects</c>), not re-added: re-adding keyed it afresh, and an object with
        /// no Guid of its own then landed beside the original rather than over it.
        /// </para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The object could not be cloned, or the clone
        /// could not be stored.</exception>
        internal static T DeepCloneObject<T>(T @object, System.Func<T, bool> isValid) where T : IJSAMObject
        {
            IJSAMObject clone = @object.Clone();
            if (clone == null)
            {
                throw new System.InvalidOperationException(string.Format(
                    "A deep copy of this cluster could not be made: '{0}' cannot be cloned. Core.Query.Clone finds no parameterless Clone() method, no constructor taking its own type, and no parameterless constructor on it - most often because it inherits a copy constructor's declaring type without declaring one of its own, which C# does not inherit. Give the type a copy constructor. Sharing the original instead would leave this copy sharing that object with the model it was copied from.",
                    @object.GetType().FullName));
            }

            if (clone is not T clone_T || !isValid(clone_T))
            {
                throw new System.InvalidOperationException(string.Format(
                    "A deep copy of this cluster could not be made: the clone of '{0}' could not be stored, so the original would have been left in its place and shared with the model this was copied from.",
                    @object.GetType().FullName));
            }

            return clone_T;
        }

        public SAMObjectRelationCluster()
            : base()
        {

        }
        public SAMObjectRelationCluster(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster sAMObjectRelationCluster)
            : this(sAMObjectRelationCluster, false)
        {

        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster sAMObjectRelationCluster, bool deepClone)
            : base(sAMObjectRelationCluster)
        {
            if (deepClone)
            {
                ReplaceObjects(x => x == null ? x : DeepCloneObject(x, IsValid));
            }
        }
    }

    public class SAMObjectRelationCluster<T> : RelationCluster<T>, IJSAMObject, ISAMObjectRelationCluster where T : IJSAMObject
    {
        public SAMObjectRelationCluster()
            : base()
        {

        }
        public SAMObjectRelationCluster(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster<T> sAMObjectRelationCluster)
            : this(sAMObjectRelationCluster, false)
        {

        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster<T> sAMObjectRelationCluster, bool deepClone)
            : base(sAMObjectRelationCluster)
        {
            if (deepClone)
            {
                ReplaceObjects(x => x == null ? x : SAMObjectRelationCluster.DeepCloneObject(x, IsValid));
            }
        }

        public virtual bool TryGetValues(IJSAMObject @object, IComplexReference complexReference, out List<object> values)
        {
            values = null;

            if (!(@object is T))
            {
                return false;
            }

            return base.TryGetValues((T)@object, complexReference, out values);
        }
    }
}
