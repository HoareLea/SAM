// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether a space is an assessed TM59 communal corridor: the internal condition it is <b>assigned</b>
        /// is exactly <see cref="TM59InternalConditionResolver.CommunalCorridorInternalConditionName"/>.
        /// <para>
        /// <b>Never from a name.</b> <c>TM59InternalConditionResolver</c> classifies from <c>space.Name</c>
        /// and so would drop a correctly assigned corridor condition on an unusually named space and accept a
        /// corridor-like name that carries no such condition. This reads the authoritative state instead
        /// (owner decision D2).
        /// </para>
        /// </summary>
        public static bool IsTM59CommunalCorridor(this Space space)
        {
            return string.Equals(space?.InternalCondition?.Name, TM59InternalConditionResolver.CommunalCorridorInternalConditionName, StringComparison.Ordinal);
        }

        /// <summary>
        /// A guid-insensitive fingerprint of one dwelling's design ventilation terminals - the guard a
        /// <c>PartODwellingStrategy</c> with <c>RetainedDesign</c> holds instead of a copy of the airflow.
        /// <para>
        /// Taken over each terminal of each space of the zone: the space's guid (a baseline identity, stable),
        /// the flow direction, the design flow, and its Approved Document F lineage (space, role and paragraph,
        /// never the requirement guid, which is re-minted by every Part F recalculation) or "authored" where it
        /// has none. Sorted, so terminal order carries no meaning; terminal guids and names are excluded, so
        /// a same-guid replacement or a renamed terminal with the same design is the same design.
        /// </para>
        /// <para>
        /// A designer accepts a design by writing it onto the baseline's terminals and recording this value on
        /// the strategy. Any later edit of a flow moves it, so the retained design is refused as stale rather
        /// than silently materialised at numbers nobody accepted.
        /// </para>
        /// </summary>
        /// <returns>The fingerprint, or null where the zone is null or has no spaces.</returns>
        public static string PartODwellingDesignFingerprint(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster is null || zone is null)
            {
                return null;
            }

            List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [];
            if (spaces.Count == 0)
            {
                return null;
            }

            List<string> lines = [];

            foreach (Space space in spaces)
            {
                if (space is null)
                {
                    continue;
                }

                foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.VentilationTerminals(space) ?? [])
                {
                    if (ventilationTerminal is null)
                    {
                        continue;
                    }

                    PartFTerminalReference partFTerminalReference = ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);

                    lines.Add(string.Join("|",
                        space.Guid.ToString("D", CultureInfo.InvariantCulture),
                        ventilationTerminal.FlowClassification.ToString(),
                        FingerprintNumber(ventilationTerminal.DesignFlowRate_Lps ?? double.NaN),
                        partFTerminalReference is null
                            ? "authored"
                            : string.Join("/", partFTerminalReference.SpaceGuid.ToString("D", CultureInfo.InvariantCulture), partFTerminalReference.TerminalRole.ToString(), partFTerminalReference.SourceReference ?? string.Empty)));
                }
            }

            return PartOFingerprint("PartODwellingDesign:v1", lines);
        }

        /// <summary>
        /// A fingerprint of every catalogue field that can change which ventilation unit a dwelling is
        /// selected: each product's identity (manufacturer, model, reference), maximum supply, maximum extract,
        /// rank and validity, in a stable order - never the product identities alone, because a capacity or
        /// rank correction changes the selection without changing any identity.
        /// <para>
        /// A null catalogue ("select nothing") is distinct from an empty one. The project test product is
        /// fingerprinted separately from the shipped catalogue, because whether it takes part is a project
        /// setting.
        /// </para>
        /// </summary>
        public static string PartOCatalogueFingerprint(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest = null)
        {
            List<string> lines = [];

            void Add(string prefix, IEnumerable<VentilationUnitCapacityDescriptor> descriptors)
            {
                if (descriptors is null)
                {
                    lines.Add(prefix + "|none");

                    return;
                }

                foreach (VentilationUnitCapacityDescriptor descriptor in descriptors)
                {
                    if (descriptor is null)
                    {
                        continue;
                    }

                    VentilationUnitReference reference = descriptor.VentilationUnitReference;

                    lines.Add(string.Join("|",
                        prefix,
                        reference?.Manufacturer ?? string.Empty,
                        reference?.Model ?? string.Empty,
                        reference?.Reference ?? string.Empty,
                        FingerprintNumber(descriptor.MaximumSupplyFlowRate_Lps),
                        FingerprintNumber(descriptor.MaximumExtractFlowRate_Lps),
                        descriptor.Rank.ToString(CultureInfo.InvariantCulture),
                        descriptor.IsValid ? "valid" : "invalid"));
                }
            }

            Add("catalogue", ventilationUnitCapacityDescriptors);
            Add("test", ventilationUnitCapacityDescriptors_ProjectTest);

            return PartOFingerprint("PartOCatalogue:v1", lines);
        }

        /// <summary>
        /// A fingerprint of the selected strategies of the assessed dwellings, over
        /// <see cref="PartODwellingStrategy.CanonicalText"/> - identity-defining state only, so two logically
        /// identical selections fingerprint identically whatever order or objects they were built from.
        /// </summary>
        public static string PartOStrategyFingerprint(IEnumerable<PartODwellingStrategy> partODwellingStrategies, IEnumerable<Guid> guids_Zone_Assessed)
        {
            List<string> lines = [];

            foreach (PartODwellingStrategy partODwellingStrategy in partODwellingStrategies ?? [])
            {
                if (partODwellingStrategy is not null)
                {
                    lines.Add("strategy|" + partODwellingStrategy.CanonicalText());
                }
            }

            foreach (Guid guid in guids_Zone_Assessed ?? [])
            {
                lines.Add("assessed|" + guid.ToString("D", CultureInfo.InvariantCulture));
            }

            return PartOFingerprint("PartOStrategies:v1", lines);
        }

        /// <summary>
        /// SHA-256 over a schema tag, the line count and the ordinally sorted lines, each length-prefixed, as
        /// lower-case hex. Reproducible across machines and processes (never <c>string.GetHashCode</c>).
        /// </summary>
        internal static string PartOFingerprint(string schema, List<string> lines)
        {
            List<string> lines_Sorted = [.. lines ?? []];
            lines_Sorted.Sort(StringComparer.Ordinal);

            StringBuilder stringBuilder = new();
            stringBuilder.Append(schema).Append('\n').Append(lines_Sorted.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');

            foreach (string line in lines_Sorted)
            {
                stringBuilder.Append(line.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(line).Append('\n');
            }

            byte[] hash;
            using (System.Security.Cryptography.SHA256 sHA256 = System.Security.Cryptography.SHA256.Create())
            {
                hash = sHA256.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            }

            StringBuilder result = new(hash.Length * 2);
            foreach (byte @byte in hash)
            {
                result.Append(@byte.ToString("x2", CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }

        /// <summary>A number as round-trippable invariant text, so a fingerprint never depends on culture.</summary>
        private static string FingerprintNumber(double value)
        {
            return double.IsNaN(value) ? "NaN" : value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
