// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// What <c>Modify.IsolateSpaces</c> produced: the derived model containing only the selected spaces as
    /// thermal zones, or - where the selection cannot be isolated at all - <b>no model and the reason</b>.
    /// <para>
    /// <b>A refusal returns no model, by contract</b>, exactly as <see cref="PartOIterationPreparation"/>
    /// does. There is no partially isolated model: a selection whose ventilation plant or airflow network
    /// reaches outside it cannot be simulated in isolation without changing what the plant is, and
    /// producing something simulatable anyway would be the silent corruption this type exists to prevent.
    /// </para>
    /// <para>
    /// <b>The refusals are reported before conversion</b>, which is the point of computing them here rather
    /// than discovering them inside TAS: a person must not wait through a long conversion to be told the
    /// scope was never valid.
    /// </para>
    /// </summary>
    public class SpaceIsolation
    {
        internal SpaceIsolation(AdjacencyCluster adjacencyCluster, List<string> refusals, List<string> notes, int count_Adiabatic, int count_Shade, int count_ApertureRemoved)
        {
            AdjacencyCluster = adjacencyCluster;

            if (refusals is not null)
            {
                Refusals.AddRange(refusals);
            }

            if (notes is not null)
            {
                Notes.AddRange(notes);
            }

            Count_AdiabaticPanel = count_Adiabatic;
            Count_ShadePanel = count_Shade;
            Count_RemovedCutAperture = count_ApertureRemoved;
        }

        /// <summary>
        /// The isolated cluster - only the selected spaces as thermal spaces. <b>Null where
        /// <see cref="Refusals"/> is not empty.</b>
        /// </summary>
        public AdjacencyCluster AdjacencyCluster { get; }

        /// <summary>
        /// Why the selection cannot be simulated in isolation. Empty where it can. Each entry names one
        /// thing - a shared unit, a shared system, an airflow path crossing the cut - so a person can see
        /// what to change rather than only that something is wrong.
        /// </summary>
        public List<string> Refusals { get; } = [];

        /// <summary>What the isolation did, for the run record - the cut, the shading context, the apertures.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Whether an isolated model was produced.</summary>
        public bool IsIsolated => AdjacencyCluster is not null && Refusals.Count == 0;

        /// <summary>How many selected-to-excluded interfaces became the adiabatic isolation cut.</summary>
        public int Count_AdiabaticPanel { get; }

        /// <summary>How many excluded external surfaces were retained as solar shading context.</summary>
        public int Count_ShadePanel { get; }

        /// <summary>How many apertures were removed because they sat on the adiabatic cut.</summary>
        public int Count_RemovedCutAperture { get; }
    }
}
