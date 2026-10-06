// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Every design ventilation terminal related to <paramref name="jSAMObject"/> - a
        /// <see cref="Space"/>, a <see cref="VentilationSystem"/> or anything else a terminal is related
        /// to.
        /// <para>
        /// <b>A list, never a single terminal.</b> A space may hold any number of supply terminals and
        /// any number of extract terminals, and a system may own the terminals of many spaces. Every
        /// caller reads the sum, never the first element - a duty read off <c>FirstOrDefault()</c> would
        /// silently under-report a subdivided room.
        /// </para>
        /// </summary>
        public static List<VentilationTerminal> VentilationTerminals(this AdjacencyCluster adjacencyCluster, Core.IJSAMObject jSAMObject)
        {
            if (adjacencyCluster is null || jSAMObject is null)
            {
                return null;
            }

            return adjacencyCluster.GetRelatedObjects<VentilationTerminal>(jSAMObject);
        }

        /// <summary>
        /// Every design terminal of one flow direction in <paramref name="ventilationTerminals"/>.
        /// </summary>
        public static List<VentilationTerminal> VentilationTerminals(this IEnumerable<VentilationTerminal> ventilationTerminals, FlowClassification flowClassification)
        {
            if (ventilationTerminals is null)
            {
                return null;
            }

            List<VentilationTerminal> result = [];

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                if (ventilationTerminal is not null && ventilationTerminal.FlowClassification == flowClassification)
                {
                    result.Add(ventilationTerminal);
                }
            }

            return result;
        }

        /// <summary>
        /// The design duty [l/s] of the terminals of one flow direction, or <b>null</b> where there is no
        /// terminal of that direction with an established duty.
        /// <para>
        /// Null rather than zero, following <c>PartFSpaceData</c>: "this room has no extract" and "this
        /// room extracts 0 l/s" are different answers, and the runtime realization has to be able to tell
        /// them apart - one means write no air movement, the other means the design says nothing yet.
        /// </para>
        /// </summary>
        public static double? VentilationTerminalDesignDuty_Lps(this IEnumerable<VentilationTerminal> ventilationTerminals, FlowClassification flowClassification)
        {
            if (ventilationTerminals is null)
            {
                return null;
            }

            double? result = null;

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                if (ventilationTerminal is null || ventilationTerminal.FlowClassification != flowClassification)
                {
                    continue;
                }

                double? designFlowRate_Lps = ventilationTerminal.DesignFlowRate_Lps;
                if (!designFlowRate_Lps.HasValue || double.IsNaN(designFlowRate_Lps.Value))
                {
                    continue;
                }

                result = (result ?? 0) + designFlowRate_Lps.Value;
            }

            return result;
        }
    }
}
