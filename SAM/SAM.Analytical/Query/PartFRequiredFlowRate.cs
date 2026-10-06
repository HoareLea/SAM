// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// What Approved Document F requires of the room one design terminal serves, recovered through
        /// the terminal's own lineage.
        /// <para>
        /// <b>Read, never stored on the terminal.</b> A copy of the requirement kept beside the design
        /// airflow would be a second answer, and the two would disagree the first time Part F was
        /// recalculated. The requirement is looked up through
        /// <see cref="VentilationTerminalParameter.PartFTerminalReference"/> every time, which is the
        /// only arrangement in which the answer cannot be stale - the same rule
        /// <see cref="VentilationSystemDesignDuty"/> follows for the duty.
        /// </para>
        /// <para>
        /// <b>This is the immutable half of the Iteration 2 invariant.</b> A design airflow may be raised
        /// above what this returns and the value here does not move; the only thing that changes it is
        /// re-running the Part F calculation. Nothing in the design or Approved Document O path writes to
        /// it.
        /// </para>
        /// <para>
        /// <b>Per requirement, not per terminal.</b> Where a room's requirement has been subdivided into
        /// several terminals, each of them recovers the <i>room's whole</i> requirement - which is what
        /// the requirement is. Summing this across a subdivided room would multiply it, so a room total
        /// is taken with <see cref="PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)"/>,
        /// which reads the space's Approved Document F data directly.
        /// </para>
        /// </summary>
        /// <returns>The continuous design rate [l/s] of the requirement, or null where the terminal realizes none.</returns>
        public static double? PartFRequiredFlowRate_Lps(this AdjacencyCluster adjacencyCluster, VentilationTerminal ventilationTerminal)
        {
            PartFVentilationTerminalRequirement partFVentilationTerminalRequirement = PartFVentilationTerminalRequirement(adjacencyCluster, ventilationTerminal);

            double? result = partFVentilationTerminalRequirement?.ContinuousDesignFlowRate_Lps;

            return result.HasValue && double.IsNaN(result.Value) ? null : result;
        }

        /// <summary>
        /// The Approved Document F requirement one design terminal was created to realize, resolved from
        /// its <see cref="PartFTerminalReference"/> through the space it serves.
        /// <para>
        /// Matched on the reference's <b>stable regulatory identity</b> - room, role and source paragraph
        /// - rather than on <c>RequirementGuid</c>, which <c>PartFCalculator</c> re-mints on every run.
        /// An ambiguous match returns nothing rather than one of the candidates, following
        /// <c>Modify.RealizePartFVentilationTerminals</c>, which refuses on the same condition.
        /// </para>
        /// </summary>
        public static PartFVentilationTerminalRequirement PartFVentilationTerminalRequirement(this AdjacencyCluster adjacencyCluster, VentilationTerminal ventilationTerminal)
        {
            if (adjacencyCluster is null || ventilationTerminal is null)
            {
                return null;
            }

            PartFTerminalReference partFTerminalReference = ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);
            if (partFTerminalReference is null)
            {
                //A terminal a designer added themselves. It realizes nothing regulatory, so there is no
                //requirement to recover - which is not the same as a requirement of zero.
                return null;
            }

            List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationTerminal) ?? [];

            foreach (Space space in spaces)
            {
                PartFSpaceData partFSpaceData = space?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

                List<PartFVentilationTerminalRequirement> requirements = (partFSpaceData?.Terminals ?? []).FindAll(partFTerminalReference.Matches);

                if (requirements.Count == 1)
                {
                    return requirements[0];
                }

                if (requirements.Count > 1)
                {
                    //Ambiguous. Returning one of them would be a guess about which regulatory requirement a
                    //terminal realizes, and Modify.RealizePartFVentilationTerminals refuses on exactly this.
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// What Approved Document F requires of one space in one direction, read from the space's own
        /// Approved Document F data.
        /// <para>
        /// The room total, and the figure a design airflow for that room is compared against. Null where
        /// the space was never sized - a corridor, a store - which is not the same as a requirement of
        /// zero.
        /// </para>
        /// <para>
        /// <b>Resolving the space is the expensive half, and a bulk caller should not pay it per space.</b>
        /// <c>GetSpaces()</c> rebuilds the model's space list on every call and the search through it is
        /// linear, so a loop over the model's spaces asking this is quadratic. A caller already traversing
        /// many spaces builds one <see cref="PartFIndex"/> and asks
        /// <see cref="PartFIndex.PartFRequiredFlowRate_Lps"/>, which resolves through that snapshot and then
        /// reads the rate with the very same reader this does. A caller asking about one space - a
        /// Grasshopper component, a single manual change - stays here and builds nothing.
        /// </para>
        /// </summary>
        public static double? PartFRequiredFlowRate_Lps(this AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            if (adjacencyCluster is null || space is null)
            {
                return null;
            }

            //Taken from the cluster rather than trusted as handed in: a caller may be holding a space from
            //before the Part F rates were applied, and that one carries a different parameter set.
            Space space_Cluster = (adjacencyCluster.GetSpaces() ?? []).Find(x => x is not null && x.Guid == space.Guid) ?? space;

            return PartFRequiredFlowRate_Lps(space_Cluster, flowClassification);
        }

        /// <summary>
        /// What Approved Document F requires of one <b>already resolved</b> space in one direction, read from
        /// that space's own Approved Document F data.
        /// <para>
        /// <b>The rule, in one place.</b> The one-space query above and <see cref="PartFIndex"/> differ only
        /// in how they find the space the model currently carries; from there both call this, so a bulk
        /// answer and a single answer cannot drift apart. Nothing here resolves anything, and callers must
        /// hand in the instance the model holds - not the one they were given.
        /// </para>
        /// </summary>
        internal static double? PartFRequiredFlowRate_Lps(Space space_Cluster, FlowClassification flowClassification)
        {
            PartFSpaceData partFSpaceData = space_Cluster?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
            if (partFSpaceData is null)
            {
                return null;
            }

            double? result = flowClassification == FlowClassification.Supply
                ? partFSpaceData.ContinuousSupplyFlowRate_Lps
                : flowClassification == FlowClassification.Extract ? partFSpaceData.ContinuousExtractFlowRate_Lps : null;

            return result.HasValue && double.IsNaN(result.Value) ? null : result;
        }

        /// <summary>
        /// What Approved Document F requires of every space a ventilation system serves, summed.
        /// <para>
        /// <b>The lower bound of the Iteration 2 invariant</b>
        /// <c>PartFRequired &lt;= Design &lt;= SelectedCapacity</c>. Derived from the spaces on every
        /// call, so it states what the Part F calculation currently says and cannot drift from it; the
        /// design duty is derived independently from the terminals by
        /// <see cref="VentilationSystemDesignDuty"/> and the two are compared, never reconciled.
        /// </para>
        /// </summary>
        /// <returns>Whether any sized space at all was found to sum.</returns>
        public static bool PartFRequiredSystemDuty(this AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, out double supplyRequirement_Lps, out double extractRequirement_Lps)
        {
            supplyRequirement_Lps = 0;
            extractRequirement_Lps = 0;

            if (adjacencyCluster is null || ventilationSystem is null)
            {
                return false;
            }

            bool result = false;

            foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
            {
                PartFSpaceData partFSpaceData = space?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    continue;
                }

                result = true;

                supplyRequirement_Lps += PartFRequiredValue(partFSpaceData.ContinuousSupplyFlowRate_Lps);
                extractRequirement_Lps += PartFRequiredValue(partFSpaceData.ContinuousExtractFlowRate_Lps);
            }

            return result;
        }

        private static double PartFRequiredValue(double? value)
        {
            return value.HasValue && !double.IsNaN(value.Value) ? value.Value : 0;
        }
    }
}
