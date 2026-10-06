// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using SAM.Math;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// A manufacturer's published performance table, exactly as published: the conditions it was measured
    /// at, and every measured value.
    /// <para>
    /// <b>Raw data, not a fitted model.</b> The whole reason this type exists is that the industry habit
    /// is to reduce a table like this to a regression, a spreadsheet slice or a single corner value early
    /// and then carry the reduction around as though it were the manufacturer's word. The legacy route
    /// this replaces did exactly that twice over - it kept one 80 l/s slice of a three-dimensional table,
    /// and it generated an equation to paste into a simulation tool. Both are derived representations,
    /// both are defensible, and neither can be checked against the brochure afterwards. Here the brochure
    /// is what is stored, and every derived thing is computed from it on demand under a named policy.
    /// </para>
    /// <para>
    /// <b>Shape is data.</b> Axes are a list, so a two-condition table and a four-condition table are the
    /// same type; outputs are a list, so a product that publishes an input power as well as a supply
    /// temperature needs no schema change. Nothing here knows what a heat recovery unit is - it is a
    /// labelled grid of numbers with a source, and the meaning lives in
    /// <see cref="VentilationUnitTemplate"/> and in whoever reads it.
    /// </para>
    /// <para>
    /// <b>Dormant in Approved Document O Iteration 2.</b> Nothing in the sizing kernel reads this. It is
    /// carried so that Iteration 3 - which has to produce an hourly leaving-air temperature and airflow
    /// for a Tas Systems model - inherits the manufacturer's own numbers rather than having to go back to
    /// the brochure. Selection uses <see cref="VentilationUnitCapacityDescriptor"/> and nothing else.
    /// </para>
    /// </summary>
    public class VentilationUnitPerformanceTable : IJSAMObject
    {
        /// <summary>
        /// The axes and outputs together, as one unit that is never mutated after it is built.
        /// <para>
        /// <b>Why this exists.</b> Axes and outputs used to be two separate fields, swapped one after the
        /// other even once that swap was moved under a lock - which made the swap atomic to a reader that
        /// also took the lock, but not to one that did not. An unsynchronized reader (every accessor below
        /// except <see cref="Interpolation(string)"/>) could still land between the two assignments and see
        /// the new axes paired with the old outputs, or vice versa. Folding both into one object collapses
        /// the swap to a single reference assignment, which is atomic to every reader with no lock required
        /// on the read side at all.
        /// </para>
        /// </summary>
        private sealed class TableState
        {
            internal readonly List<VentilationUnitPerformanceAxis> Axes;
            internal readonly List<VentilationUnitPerformanceOutput> Outputs;

            internal TableState(List<VentilationUnitPerformanceAxis> axes, List<VentilationUnitPerformanceOutput> outputs)
            {
                Axes = axes;
                Outputs = outputs;
            }
        }

        /// <summary>
        /// The published axes/outputs, as one <see cref="TableState"/>. <c>volatile</c> so a reload's
        /// reference assignment - see <see cref="FromJsonObject"/> - is visible to a reader on another
        /// thread that never takes <see cref="@lock"/>, without which nothing would guarantee the write
        /// is observed rather than a stale cached copy of the reference.
        /// <para>
        /// <b>Every read operation captures this exactly once</b> - into a local, at the top of the
        /// operation - and does the rest of its work from that one snapshot. Reading the field a second
        /// time partway through an operation (for instance calling one public accessor from inside
        /// another) would reopen exactly the tearing window this field exists to close, just one level up:
        /// axes read under one published state, outputs - or an index derived from the axes - read under a
        /// different one.
        /// </para>
        /// </summary>
        private volatile TableState state = new(null, null);

        /// <summary>
        /// The interpolators, one per output, built once from the axes and cached. Rebuilt from scratch
        /// whenever the table is reloaded, so a cached interpolator can never outlive the data it came
        /// from.
        /// </summary>
        private Dictionary<string, Math.MultilinearInterpolation> multilinearInterpolations;

        /// <summary>
        /// Guards the cache above, and coordinates <see cref="Interpolation(string)"/>'s stale-build check
        /// against <see cref="FromJsonObject"/>'s reload. Iteration 3 is expected to read one shared
        /// catalogue table across eight thousand timesteps, and a Dictionary written from two threads at
        /// once corrupts silently - which on a performance lookup means wrong numbers rather than an
        /// exception. The lock is taken only while the cache is consulted, never while a value is
        /// interpolated.
        /// </summary>
        private readonly object @lock = new();

        /// <summary>
        /// Test-only seam. Invoked, outside any lock, immediately after <see cref="Interpolation(string)"/>
        /// captures its pre-build snapshot and before it builds the interpolator - the exact window a
        /// concurrent <see cref="FromJsonObject"/> reload occupies in the race <see cref="state"/>'s own
        /// reference identity closes. Production code never sets this.
        /// </summary>
        internal Action OnInterpolationSnapshotCaptured;

        /// <summary>
        /// Test-only seam. Invoked after <see cref="FromJsonObject"/> has finished parsing the replacement
        /// axes/outputs into a new <see cref="TableState"/> but before it publishes it - the window in
        /// which a concurrent reader must still see the complete pre-reload table. Production code never
        /// sets this.
        /// </summary>
        internal Action OnReplacementLocalsPrepared;

        public VentilationUnitPerformanceTable()
        {
        }

        public VentilationUnitPerformanceTable(IEnumerable<VentilationUnitPerformanceAxis> axes, IEnumerable<VentilationUnitPerformanceOutput> outputs)
        {
            state = Copied(axes, outputs);
        }

        public VentilationUnitPerformanceTable(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            //One read of the source table's published state, so a concurrent reload on the SOURCE cannot
            //hand this copy axes from one load and outputs from another - the same single-snapshot rule
            //every reader below follows.
            TableState sourceState = ventilationUnitPerformanceTable?.state;

            state = Copied(sourceState?.Axes, sourceState?.Outputs);
        }

        public VentilationUnitPerformanceTable(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>How many independent conditions the table is tabulated against. -1 where it holds nothing.</summary>
        public int AxisCount
        {
            get
            {
                List<VentilationUnitPerformanceAxis> axes = state.Axes;
                return axes is null ? -1 : axes.Count;
            }
        }

        /// <summary>The names of the axes, in order.</summary>
        public List<string> AxisNames
        {
            get
            {
                return Names(state.Axes);
            }
        }

        /// <summary>The names of the outputs, in order.</summary>
        public List<string> OutputNames
        {
            get
            {
                return Names(state.Outputs);
            }
        }

        /// <summary>How many conditions the table was measured at - the product of the axis lengths.</summary>
        public int PointCount
        {
            get
            {
                return PointCountOf(state.Axes);
            }
        }

        /// <summary>
        /// Whether this is a table a lookup can be answered from.
        /// <para>
        /// Every axis and every output has to be valid in its own right, every axis name has to be
        /// distinct, every output name has to be distinct, and - the check that matters most - every
        /// output has to hold exactly one value per grid point. An output one value short of the grid is
        /// not a smaller table; it is a table whose every value from the mistake onwards is attributed to
        /// the wrong conditions, and there is no way to detect that later.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get
            {
                TableState state = this.state;
                return IsValidState(state.Axes, state.Outputs);
            }
        }

        /// <summary>The position of a named axis, or -1 where the table has no such axis.</summary>
        public int AxisIndex(string name)
        {
            return AxisIndexIn(state.Axes, name);
        }

        /// <summary>One axis by position, or null. A copy.</summary>
        public VentilationUnitPerformanceAxis Axis(int index)
        {
            return AxisAt(state.Axes, index);
        }

        /// <summary>One axis by name, or null. A copy.</summary>
        public VentilationUnitPerformanceAxis Axis(string name)
        {
            //ONE capture of the axes, so the index this resolves the name to and the axis it hands back are
            //read from the SAME list - calling AxisIndex(name) and then Axis(index) as two separate calls
            //would read state.Axes twice, and a reload landing between the two could resolve an index that
            //belongs to a different axis in the second read's list.
            List<VentilationUnitPerformanceAxis> axes = state.Axes;
            return AxisAt(axes, AxisIndexIn(axes, name));
        }

        /// <summary>One output by name, or null. A copy.</summary>
        public VentilationUnitPerformanceOutput Output(string name)
        {
            VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput = FindOutput(state.Outputs, name);
            return ventilationUnitPerformanceOutput is null ? null : new VentilationUnitPerformanceOutput(ventilationUnitPerformanceOutput);
        }

        /// <summary>
        /// One published value, addressed by its position on each axis - the number as transcribed from the
        /// document, with no arithmetic performed on it.
        /// <para>
        /// This is what a test pins a brochure figure with, and what a report quotes. It is deliberately a
        /// differently named method from <see cref="Value(string, double[], PerformanceDomainPolicy)"/>:
        /// "the manufacturer says 15.7" and "we worked out 15.7" are different claims, and the two must
        /// never be reachable by an accident of overload resolution.
        /// </para>
        /// </summary>
        public double PublishedValue(string outputName, params int[] indices)
        {
            //ONE capture for the whole lookup - axes, validity and the output all come from the SAME
            //published state, so a reload landing mid-call cannot hand this method one generation's axis
            //layout and a different generation's output values.
            TableState state = this.state;
            List<VentilationUnitPerformanceAxis> axes = state.Axes;
            List<VentilationUnitPerformanceOutput> outputs = state.Outputs;

            if (!IsValidState(axes, outputs) || indices is null || indices.Length != axes.Count)
            {
                return double.NaN;
            }

            VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput = FindOutput(outputs, outputName);
            if (ventilationUnitPerformanceOutput is null)
            {
                return double.NaN;
            }

            int index = 0;

            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < 0 || indices[i] >= axes[i].Count)
                {
                    return double.NaN;
                }

                index = (index * axes[i].Count) + indices[i];
            }

            double[] values = ventilationUnitPerformanceOutput.Values;

            return index >= 0 && index < values.Length ? values[index] : double.NaN;
        }

        /// <summary>
        /// The value of one output at one set of conditions.
        /// <para>
        /// At a published condition this returns the published number exactly - the interpolation weights
        /// are exactly zero and one there. Between published conditions it is multilinear over the
        /// surrounding cell, which is deterministic and independent of how the table was ordered or read.
        /// </para>
        /// <para>
        /// <b>Outside the published conditions the answer depends on <paramref name="performanceDomainPolicy"/>,
        /// and the default is to refuse.</b> See <see cref="PerformanceDomainPolicy"/> - the point is that
        /// a number the manufacturer never published can never be produced by accident.
        /// </para>
        /// </summary>
        /// <param name="outputName">Which published quantity - see <c>VentilationUnitPerformanceOutput.Name_</c>.</param>
        /// <param name="coordinates">One coordinate per axis, in axis order.</param>
        /// <param name="performanceDomainPolicy">What to do beyond the published conditions.</param>
        public double Value(string outputName, double[] coordinates, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            Math.MultilinearInterpolation multilinearInterpolation = Interpolation(outputName);

            if (multilinearInterpolation is null || coordinates is null)
            {
                return double.NaN;
            }

            switch (performanceDomainPolicy)
            {
                case PerformanceDomainPolicy.ClampToDomain:
                    return multilinearInterpolation.CalculateClamped(coordinates);

                case PerformanceDomainPolicy.OuterCellLinearExtrapolation:
                    return multilinearInterpolation.CalculateExtrapolated(coordinates);

                default:
                    //Undefined falls here with Refuse. An unrecognised policy is not a licence to extrapolate,
                    //and defaulting the other way would make a forgotten argument the permissive one.
                    return multilinearInterpolation.Calculate(coordinates);
            }
        }

        /// <summary>
        /// Whether a set of conditions falls inside everything the manufacturer published. Conditions
        /// exactly on a published boundary are inside it.
        /// </summary>
        public bool InDomain(params double[] coordinates)
        {
            //ONE capture - see PublishedValue's remarks. The domain this checks against has to be the same
            //axes IsValidState just proved were self-consistent.
            TableState state = this.state;
            List<VentilationUnitPerformanceAxis> axes = state.Axes;

            if (!IsValidState(axes, state.Outputs) || coordinates is null || coordinates.Length != axes.Count)
            {
                return false;
            }

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (double.IsNaN(coordinates[i]) || coordinates[i] < axes[i].Minimum || coordinates[i] > axes[i].Maximum)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The interpolator behind one output, built from the axes and cached. Null where the table is not
        /// valid or has no such output.
        /// <para>
        /// <b>Private.</b> Every read goes through <see cref="Value(string, double[], PerformanceDomainPolicy)"/>,
        /// which is the method that also applies the domain policy - so there is no way to reach the
        /// arithmetic while bypassing the decision about what happens outside the published range. It also
        /// keeps <c>SAM.Math</c> out of this type's public surface.
        /// </para>
        /// </summary>
        private Math.MultilinearInterpolation Interpolation(string outputName)
        {
            if (string.IsNullOrWhiteSpace(outputName))
            {
                return null;
            }

            Math.MultilinearInterpolation result;
            TableState stateSnapshot;

            lock (@lock)
            {
                multilinearInterpolations ??= [];

                if (multilinearInterpolations.TryGetValue(outputName, out result))
                {
                    return result;
                }

                //ONE capture of the published state - axes and outputs together, from the same reload -
                //taken under the same lock a reload also swaps its reference under. state's own identity
                //stands in for the old separate generation counter: this snapshot is either entirely the
                //pre-reload TableState or entirely the post-reload one, and "is it still current" below is
                //answered by comparing THIS OBJECT to whatever state now holds, not by comparing numbers.
                stateSnapshot = state;
            }

            if (!IsValidState(stateSnapshot.Axes, stateSnapshot.Outputs))
            {
                return null;
            }

            VentilationUnitPerformanceOutput outputSnapshot = FindOutput(stateSnapshot.Outputs, outputName);
            if (outputSnapshot is null)
            {
                return null;
            }

            //Test-only: lets a test force a FromJsonObject reload here, deterministically, in the exact
            //window a concurrent one would otherwise race this build in.
            OnInterpolationSnapshotCaptured?.Invoke();

            List<double[]> axisValues = [];
            foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in stateSnapshot.Axes)
            {
                axisValues.Add(ventilationUnitPerformanceAxis.Values);
            }

            result = new Math.MultilinearInterpolation(axisValues, outputSnapshot.Values);

            if (!result.IsValid)
            {
                return null;
            }

            lock (@lock)
            {
                if (!ReferenceEquals(state, stateSnapshot))
                {
                    //A reload published a new TableState while this interpolator was being built from the
                    //pre-reload snapshot above. The result is correct for the table it was built from, but
                    //that table is no longer the published one - it must not go into the current cache, or
                    //a later lookup for the same output would silently read the old table's value.
                    return result;
                }

                //Re-created rather than assumed: a concurrent reload will have cleared it, and an interpolator
                //this call already built is still worth returning to its caller.
                multilinearInterpolations ??= [];

                //Last writer wins, and that is harmless: two threads racing here on the SAME published state
                //build interpolators that are equal in every value, because both read the same immutable
                //axes and output.
                multilinearInterpolations[outputName] = result;
            }

            return result;
        }

        public override string ToString()
        {
            TableState state = this.state;

            if (!IsValidState(state.Axes, state.Outputs))
            {
                return "Invalid VentilationUnitPerformanceTable";
            }

            return string.Format("{0} point(s) over {1}, publishing {2}", PointCountOf(state.Axes), string.Join(" x ", Names(state.Axes)), string.Join(", ", Names(state.Outputs)));
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            //Parsed into locals first - never onto any field a reader could see - so a concurrent reader is
            //never exposed to a state nulled out or half-built from the array currently being walked.
            List<VentilationUnitPerformanceAxis> axes_New = null;
            List<VentilationUnitPerformanceOutput> outputs_New = null;

            if (jsonObject["Axes"] is JsonArray jsonArray_Axes)
            {
                axes_New = [];
                foreach (JsonNode jsonNode in jsonArray_Axes)
                {
                    axes_New.Add(jsonNode is JsonObject jsonObject_Axis ? new VentilationUnitPerformanceAxis(jsonObject_Axis) : null);
                }
            }

            if (jsonObject["Outputs"] is JsonArray jsonArray_Outputs)
            {
                outputs_New = [];
                foreach (JsonNode jsonNode in jsonArray_Outputs)
                {
                    outputs_New.Add(jsonNode is JsonObject jsonObject_Output ? new VentilationUnitPerformanceOutput(jsonObject_Output) : null);
                }
            }

            //Test-only: lets a test observe the pre-publish state - still the complete old TableState -
            //from the exact window a concurrent reload used to be able to publish a torn one in.
            OnReplacementLocalsPrepared?.Invoke();

            TableState state_New = new(axes_New, outputs_New);

            lock (@lock)
            {
                //Published through ONE reference assignment - axes and outputs together, as the single
                //TableState built above - so every reader, synchronized or not, sees either the complete
                //pre-reload state or the complete post-reload one, never axes from one paired with outputs
                //from the other. Assigning a volatile field is itself an atomic, ordered publish; the lock
                //here is for multilinearInterpolations below, which Interpolation also only ever touches
                //under this same lock.
                state = state_New;

                //Cleared with the data it was built from, so a cached interpolator can never outlive it.
                multilinearInterpolations = null;
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            //ONE capture, so a table mid-reload serializes as either the complete old table or the complete
            //new one, never new axes written out beside old outputs.
            TableState state = this.state;

            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (state.Axes is not null)
            {
                JsonArray jsonArray = [];
                foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in state.Axes)
                {
                    jsonArray.Add(ventilationUnitPerformanceAxis?.ToJsonObject());
                }

                result["Axes"] = jsonArray;
            }

            if (state.Outputs is not null)
            {
                JsonArray jsonArray = [];
                foreach (VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput in state.Outputs)
                {
                    jsonArray.Add(ventilationUnitPerformanceOutput?.ToJsonObject());
                }

                result["Outputs"] = jsonArray;
            }

            return result;
        }

        /// <summary>Deep-copies a pair of axes/outputs into a new <see cref="TableState"/> - both constructors that take data directly route through here.</summary>
        private static TableState Copied(IEnumerable<VentilationUnitPerformanceAxis> axes, IEnumerable<VentilationUnitPerformanceOutput> outputs)
        {
            //Copied: both are mutable-ish reference types, and a table a lookup is reading must not change
            //underneath it - the same rule VentilationUnitCapacityDescriptor follows for its reference.
            List<VentilationUnitPerformanceAxis> axes_New = null;
            if (axes is not null)
            {
                axes_New = [];
                foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in axes)
                {
                    axes_New.Add(ventilationUnitPerformanceAxis is null ? null : new VentilationUnitPerformanceAxis(ventilationUnitPerformanceAxis));
                }
            }

            List<VentilationUnitPerformanceOutput> outputs_New = null;
            if (outputs is not null)
            {
                outputs_New = [];
                foreach (VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput in outputs)
                {
                    outputs_New.Add(ventilationUnitPerformanceOutput is null ? null : new VentilationUnitPerformanceOutput(ventilationUnitPerformanceOutput));
                }
            }

            return new TableState(axes_New, outputs_New);
        }

        /// <summary>Whether an explicit axes/outputs pair - both drawn from the SAME captured state - is one a lookup can be answered from. See <see cref="IsValid"/>.</summary>
        private static bool IsValidState(List<VentilationUnitPerformanceAxis> axes, List<VentilationUnitPerformanceOutput> outputs)
        {
            if (axes is null || axes.Count == 0 || outputs is null || outputs.Count == 0)
            {
                return false;
            }

            int pointCount = PointCountOf(axes);
            if (pointCount <= 0)
            {
                return false;
            }

            HashSet<string> names = [];

            foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in axes)
            {
                if (ventilationUnitPerformanceAxis is null || !ventilationUnitPerformanceAxis.IsValid || !names.Add(ventilationUnitPerformanceAxis.Name))
                {
                    return false;
                }
            }

            names = [];

            foreach (VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput in outputs)
            {
                if (ventilationUnitPerformanceOutput is null || !ventilationUnitPerformanceOutput.IsValid || !names.Add(ventilationUnitPerformanceOutput.Name))
                {
                    return false;
                }

                if (ventilationUnitPerformanceOutput.Count != pointCount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The product of an explicit axes list's lengths. See <see cref="PointCount"/>.</summary>
        private static int PointCountOf(List<VentilationUnitPerformanceAxis> axes)
        {
            if (axes is null || axes.Count == 0)
            {
                return -1;
            }

            long result = 1;

            foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in axes)
            {
                if (ventilationUnitPerformanceAxis is null || ventilationUnitPerformanceAxis.Count <= 0)
                {
                    return -1;
                }

                result *= ventilationUnitPerformanceAxis.Count;

                if (result > int.MaxValue)
                {
                    return -1;
                }
            }

            return (int)result;
        }

        /// <summary>The names of an explicit axis list, in order. See <see cref="AxisNames"/>.</summary>
        private static List<string> Names(List<VentilationUnitPerformanceAxis> axes)
        {
            if (axes is null)
            {
                return null;
            }

            List<string> result = [];

            foreach (VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis in axes)
            {
                result.Add(ventilationUnitPerformanceAxis?.Name);
            }

            return result;
        }

        /// <summary>The names of an explicit output list, in order. See <see cref="OutputNames"/>.</summary>
        private static List<string> Names(List<VentilationUnitPerformanceOutput> outputs)
        {
            if (outputs is null)
            {
                return null;
            }

            List<string> result = [];

            foreach (VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput in outputs)
            {
                result.Add(ventilationUnitPerformanceOutput?.Name);
            }

            return result;
        }

        /// <summary>The position of a named axis in an explicit axes list, or -1. See <see cref="AxisIndex"/>.</summary>
        private static int AxisIndexIn(List<VentilationUnitPerformanceAxis> axes, string name)
        {
            if (axes is null)
            {
                return -1;
            }

            for (int i = 0; i < axes.Count; i++)
            {
                if (axes[i] is not null && axes[i].Matches(name))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>One axis by position in an explicit axes list, or null. A copy. See <see cref="Axis(int)"/>.</summary>
        private static VentilationUnitPerformanceAxis AxisAt(List<VentilationUnitPerformanceAxis> axes, int index)
        {
            if (axes is null || index < 0 || index >= axes.Count || axes[index] is null)
            {
                return null;
            }

            return new VentilationUnitPerformanceAxis(axes[index]);
        }

        /// <summary>The stored output by name in an explicit outputs list, uncopied - for the internals that only read it.</summary>
        private static VentilationUnitPerformanceOutput FindOutput(List<VentilationUnitPerformanceOutput> outputs, string name)
        {
            if (outputs is null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput in outputs)
            {
                if (ventilationUnitPerformanceOutput is not null && ventilationUnitPerformanceOutput.Matches(name))
                {
                    return ventilationUnitPerformanceOutput;
                }
            }

            return null;
        }
    }
}
