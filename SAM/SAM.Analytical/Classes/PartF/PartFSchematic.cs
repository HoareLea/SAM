// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SAM.Analytical
{
    /// <summary>
    /// Draws a dwelling's airflow as a compact text schematic: where outdoor air enters, what each room
    /// supplies and extracts, and how much transfer air crosses each internal door on the way to an
    /// extract terminal.
    /// <para>
    /// Plain text and no user interface dependency at all, deliberately. The same renderer feeds the
    /// report window, the clipboard, an exported file, a Grasshopper output and the regression tests, so
    /// every one of them shows the same diagram, and a change to it is caught by a test rather than by
    /// somebody noticing on screen.
    /// </para>
    /// <para>
    /// The four operating conditions are drawn separately and never mixed. A single number that was
    /// partly continuous design, partly boost and partly setback would describe an operating state the
    /// system never enters, so each schematic names its own condition in its heading.
    /// </para>
    /// <para>
    /// The l/s figure drawn on a transfer branch is SAM's <b>calculated airflow-network routing</b>, from
    /// conserving air across the dwelling. Approved Document F paragraph 1.25 requires a free area
    /// through an internal door and prescribes no flow rate for any individual door, so nothing on a
    /// branch here is a Part F door-flow requirement. The paragraph 1.25 assessment is on free area, in
    /// <see cref="PartFDoorTransferData"/>.
    /// </para>
    /// </summary>
    public static class PartFSchematic
    {
        //Drawing characters as escapes rather than literals, so the source file stays plain ASCII and no
        //encoding step between here and the compiler can quietly alter the diagram.

        /// <summary>Minus sign, U+2212. Used for air leaving a space; a hyphen would read as a dash.</summary>
        public const string Minus = "−";

        /// <summary>Downwards arrow, U+2193.</summary>
        public const string ArrowDown = "↓";

        /// <summary>Rightwards arrow, U+2192.</summary>
        public const string ArrowRight = "→";

        /// <summary>Box drawings light vertical, U+2502.</summary>
        public const string Vertical = "│";

        /// <summary>Box drawings light horizontal, U+2500.</summary>
        public const string Horizontal = "─";

        /// <summary>Box drawings light up and right, U+2514. The last branch out of a space.</summary>
        public const string CornerLast = "└";

        /// <summary>Box drawings light vertical and right, U+251C. A branch with more to follow.</summary>
        public const string CornerTee = "├";

        /// <summary>Em dash, U+2014. Separates the schematic heading from its operating condition.</summary>
        public const string EmDash = "—";

        /// <summary>The label at the top of every schematic.</summary>
        public const string OutdoorSupply = "Outdoor supply";

        /// <summary>Label used where two spaces are adjacent but no door aperture is modelled.</summary>
        public const string UnnamedDoor = "internal door";

        /// <summary>Indent of the first level of branches, matching the width under the arrow above it.</summary>
        private const string indent_Root = "      ";

        /// <summary>
        /// Renders the schematic of one dwelling at one operating condition.
        /// </summary>
        public static string Build(PartFComplianceResult partFComplianceResult, PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign)
        {
            StringBuilder stringBuilder = new();

            if (partFComplianceResult is null)
            {
                return string.Empty;
            }

            stringBuilder.AppendLine(Heading(partFOperatingMode));
            stringBuilder.AppendLine();

            Dictionary<Guid, string> dictionary_Name = [];
            Dictionary<Guid, double> dictionary_Supply = [];
            Dictionary<Guid, double> dictionary_LocalKitchenExtract = [];
            Dictionary<Guid, double> dictionary_GeneralExtract = [];

            foreach (PartFVentilationTerminalRequirement terminal in partFComplianceResult.Terminals ?? [])
            {
                dictionary_Name[terminal.SpaceGuid] = terminal.SpaceName;

                double? rate = Rate(terminal, partFOperatingMode);
                if (rate is null)
                {
                    continue;
                }

                Dictionary<Guid, double> dictionary = terminal.TerminalRole switch
                {
                    PartFTerminalRole.Supply => dictionary_Supply,
                    PartFTerminalRole.LocalKitchenExtract => dictionary_LocalKitchenExtract,
                    _ => dictionary_GeneralExtract,
                };

                dictionary.TryGetValue(terminal.SpaceGuid, out double value);
                dictionary[terminal.SpaceGuid] = value + rate.Value;
            }

            List<PartFDoorTransferData> transferPaths = [.. partFComplianceResult.TransferPaths ?? []];

            foreach (PartFDoorTransferData partFDoorTransferData in transferPaths)
            {
                //A space with no terminal at all - a hall, a landing - only appears in the diagram because
                //air passes through it, so its name comes from the transfer schedule.
                if (!dictionary_Name.ContainsKey(partFDoorTransferData.UpstreamSpaceGuid))
                {
                    dictionary_Name[partFDoorTransferData.UpstreamSpaceGuid] = partFDoorTransferData.UpstreamSpaceName;
                }

                if (!dictionary_Name.ContainsKey(partFDoorTransferData.DownstreamSpaceGuid))
                {
                    dictionary_Name[partFDoorTransferData.DownstreamSpaceGuid] = partFDoorTransferData.DownstreamSpaceName;
                }
            }

            //The spaces air starts from: everything with more supply than extract at this condition. They
            //are the roots of the diagram, because that is the direction air actually moves.
            List<Guid> guids_Root = [.. dictionary_Name.Keys
                .Where(x => Net(x, dictionary_Supply, dictionary_LocalKitchenExtract, dictionary_GeneralExtract) > PartFAirflowNetwork.Tolerance_Lps)
                .OrderBy(x => dictionary_Name[x], StringComparer.Ordinal)];

            if (guids_Root.Count == 0)
            {
                //Nothing has air to pass on. A one-room dwelling whose only terminal is its own extract is
                //a legitimate case and is drawn as a list rather than as a tree.
                stringBuilder.AppendLine(OutdoorSupply);
                stringBuilder.AppendLine(indent_Root + ArrowDown);

                foreach (Guid guid in dictionary_Name.Keys.OrderBy(x => dictionary_Name[x], StringComparer.Ordinal))
                {
                    stringBuilder.AppendLine(SpaceText(guid, dictionary_Name, dictionary_Supply, dictionary_LocalKitchenExtract, dictionary_GeneralExtract));
                }

                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine(OutdoorSupply);
            stringBuilder.AppendLine(indent_Root + ArrowDown);

            for (int i = 0; i < guids_Root.Count; i++)
            {
                if (i != 0)
                {
                    stringBuilder.AppendLine();
                }

                Guid guid_Root = guids_Root[i];

                stringBuilder.AppendLine(SpaceText(guid_Root, dictionary_Name, dictionary_Supply, dictionary_LocalKitchenExtract, dictionary_GeneralExtract));

                AppendBranches(
                    stringBuilder,
                    guid_Root,
                    indent_Root,
                    [guid_Root],
                    transferPaths,
                    partFOperatingMode,
                    dictionary_Name,
                    dictionary_Supply,
                    dictionary_LocalKitchenExtract,
                    dictionary_GeneralExtract);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// The per-space airflow breakdown that sits beside the schematic: what each space is given, what
        /// is taken from it, and the net transfer that follows.
        /// </summary>
        public static string BuildSpaceAirflow(PartFComplianceResult partFComplianceResult, PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign)
        {
            StringBuilder stringBuilder = new();

            if (partFComplianceResult is null)
            {
                return string.Empty;
            }

            List<IGrouping<Guid, PartFVentilationTerminalRequirement>> groupings = [.. (partFComplianceResult.Terminals ?? [])
                .GroupBy(x => x.SpaceGuid)
                .OrderBy(x => x.First().SpaceName, StringComparer.Ordinal)];

            foreach (IGrouping<Guid, PartFVentilationTerminalRequirement> grouping in groupings)
            {
                stringBuilder.AppendLine(string.Format("{0}:", grouping.First().SpaceName));

                double net = 0;

                foreach (PartFVentilationTerminalRequirement terminal in grouping.OrderBy(x => x.TerminalRole))
                {
                    double? rate = Rate(terminal, partFOperatingMode);
                    if (rate is null)
                    {
                        stringBuilder.AppendLine(string.Format("  {0,-24}{1}", RoleText(terminal.TerminalRole) + ":", "not applicable at this condition"));
                        continue;
                    }

                    //An intermittent device does not run at the continuous design condition, so it is shown
                    //but excluded from the net, which is what the transfer air is worked out from.
                    bool counted = terminal.IsInBalancedFlow;

                    if (counted)
                    {
                        net += terminal.IsExtract ? -rate.Value : rate.Value;
                    }

                    stringBuilder.AppendLine(string.Format("  {0,-24}{1}{2}", RoleText(terminal.TerminalRole) + ":", Signed(terminal.IsExtract ? -rate.Value : rate.Value), counted ? string.Empty : "   (intermittent, outside the balanced flow)"));
                }

                stringBuilder.AppendLine(string.Format("  {0,-24}{1}", "Net transfer:", Signed(net)));
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        /// <summary>The heading line of a schematic, naming its operating condition.</summary>
        public static string Heading(PartFOperatingMode partFOperatingMode)
        {
            string condition = partFOperatingMode switch
            {
                PartFOperatingMode.ContinuousDesign => "CONTINUOUS DESIGN",
                PartFOperatingMode.HighBoost => "HIGH/BOOST",
                PartFOperatingMode.Setback => "SETBACK",
                PartFOperatingMode.MeasuredCommissioning => "MEASURED COMMISSIONING",
                _ => partFOperatingMode.ToString().ToUpperInvariant(),
            };

            return string.Format("AIRFLOW SCHEMATIC {0} {1}", EmDash, condition);
        }

        private static void AppendBranches(
            StringBuilder stringBuilder,
            Guid guid,
            string indent,
            HashSet<Guid> guids_Visited,
            List<PartFDoorTransferData> transferPaths,
            PartFOperatingMode partFOperatingMode,
            Dictionary<Guid, string> dictionary_Name,
            Dictionary<Guid, double> dictionary_Supply,
            Dictionary<Guid, double> dictionary_LocalKitchenExtract,
            Dictionary<Guid, double> dictionary_GeneralExtract)
        {
            //Only routes carrying air away from this space become branches, so the diagram reads as the
            //path air takes rather than as a floor plan. Measured commissioning has no transfer
            //measurement, so at that condition every route is drawn.
            List<PartFDoorTransferData> branches = [.. transferPaths
                .Where(x => x.UpstreamSpaceGuid == guid && !guids_Visited.Contains(x.DownstreamSpaceGuid))
                .Where(x => partFOperatingMode == PartFOperatingMode.MeasuredCommissioning || (Rate(x, partFOperatingMode) ?? 0) > PartFAirflowNetwork.Tolerance_Lps)
                .OrderBy(x => x.DownstreamSpaceName, StringComparer.Ordinal)
                .ThenBy(x => x.Name, StringComparer.Ordinal)];

            for (int i = 0; i < branches.Count; i++)
            {
                PartFDoorTransferData partFDoorTransferData = branches[i];

                bool isLast = i == branches.Count - 1;

                stringBuilder.AppendLine(indent + Vertical);

                string arrow = string.Concat(Enumerable.Repeat(Horizontal, 4));

                string prefix = string.Format("{0}{1}{2} {3} {2}{4} ",
                    indent,
                    isLast ? CornerLast : CornerTee,
                    arrow,
                    RouteText(partFDoorTransferData, partFOperatingMode),
                    ArrowRight);

                stringBuilder.AppendLine(prefix + SpaceText(partFDoorTransferData.DownstreamSpaceGuid, dictionary_Name, dictionary_Supply, dictionary_LocalKitchenExtract, dictionary_GeneralExtract));

                //Said on its own line, under the branch, where a reader cannot miss it. The schematic
                //used to write "through internal door" for every route including ones the model has no
                //door for, which asserted an opening the assessment was simultaneously reporting as
                //absent.
                string caption = CaptionText(partFDoorTransferData);
                if (caption is not null)
                {
                    stringBuilder.AppendLine(string.Concat(indent, isLast ? " " : Vertical, new string(' ', arrow.Length + 2), caption));
                }

                //A cycle in the dwelling's connections would otherwise redraw the same rooms for ever. The
                //visited set is per root branch, so a diamond is drawn once down each side rather than
                //repeated.
                HashSet<Guid> guids_Visited_Branch = [.. guids_Visited, partFDoorTransferData.DownstreamSpaceGuid];

                AppendBranches(
                    stringBuilder,
                    partFDoorTransferData.DownstreamSpaceGuid,
                    new string(' ', prefix.Length),
                    guids_Visited_Branch,
                    transferPaths,
                    partFOperatingMode,
                    dictionary_Name,
                    dictionary_Supply,
                    dictionary_LocalKitchenExtract,
                    dictionary_GeneralExtract);
            }
        }

        private static double Net(Guid guid, Dictionary<Guid, double> dictionary_Supply, Dictionary<Guid, double> dictionary_LocalKitchenExtract, Dictionary<Guid, double> dictionary_GeneralExtract)
        {
            dictionary_Supply.TryGetValue(guid, out double supply);
            dictionary_LocalKitchenExtract.TryGetValue(guid, out double localKitchenExtract);
            dictionary_GeneralExtract.TryGetValue(guid, out double generalExtract);

            return supply - localKitchenExtract - generalExtract;
        }

        /// <summary>
        /// One space as it appears in the diagram: its name, then whichever of supply, local kitchen
        /// extract and general extract it actually has. A space with no terminal - a hall, a landing -
        /// appears as its name alone, which is exactly what it contributes.
        /// </summary>
        private static string SpaceText(
            Guid guid,
            Dictionary<Guid, string> dictionary_Name,
            Dictionary<Guid, double> dictionary_Supply,
            Dictionary<Guid, double> dictionary_LocalKitchenExtract,
            Dictionary<Guid, double> dictionary_GeneralExtract)
        {
            string name = dictionary_Name.TryGetValue(guid, out string value) ? value : guid.ToString();

            List<string> parts = [];

            if (dictionary_Supply.TryGetValue(guid, out double supply) && System.Math.Abs(supply) > PartFAirflowNetwork.Tolerance_Lps)
            {
                parts.Add(string.Format("+{0} l/s supply", Number(supply)));
            }

            if (dictionary_LocalKitchenExtract.TryGetValue(guid, out double localKitchenExtract) && System.Math.Abs(localKitchenExtract) > PartFAirflowNetwork.Tolerance_Lps)
            {
                parts.Add(string.Format("{0}{1} l/s local kitchen extract", Minus, Number(localKitchenExtract)));
            }

            if (dictionary_GeneralExtract.TryGetValue(guid, out double generalExtract) && System.Math.Abs(generalExtract) > PartFAirflowNetwork.Tolerance_Lps)
            {
                parts.Add(string.Format("{0}{1} l/s extract", Minus, Number(generalExtract)));
            }

            return parts.Count == 0 ? name : string.Format("{0}: {1}", name, string.Join(", ", parts));
        }

        /// <summary>
        /// The text on a transfer branch, which says what the route rests on as well as how much air it
        /// carries.
        /// <para>
        /// It obeys <see cref="PartFDoorTransferData.OpeningStatus"/>, and the wording "through" is
        /// reserved for a route that has something to go through. A schematic that says "63 l/s through
        /// internal door" for an adjacency the model carries no door for contradicts the same
        /// assessment's own door schedule, and the reader has no way to tell which is right.
        /// </para>
        /// </summary>
        private static string RouteText(PartFDoorTransferData partFDoorTransferData, PartFOperatingMode partFOperatingMode)
        {
            string flow = TransferText(partFDoorTransferData, partFOperatingMode);

            switch (partFDoorTransferData.OpeningStatus)
            {
                case Enums.PartFTransferOpeningStatus.MissingTransferOpening:
                    return string.Format("{0} calculated transfer ?", flow);

                case Enums.PartFTransferOpeningStatus.AmbiguousRoute:
                    return string.Format("{0} calculated transfer ?", flow);

                case Enums.PartFTransferOpeningStatus.CalculatedViaPermanentOpening:
                    return string.Format("{0} through {1}", flow, Core.Query.Description(partFDoorTransferData.TransferDeviceType).ToLowerInvariant());

                default:
                    //A modelled door is named; there is a real door to point at on a drawing.
                    return string.Format("{0} through {1}", flow, partFDoorTransferData.IsDoorRepresented && !string.IsNullOrWhiteSpace(partFDoorTransferData.Name)
                        ? partFDoorTransferData.Name
                        : UnnamedDoor);
            }
        }

        /// <summary>
        /// The line under a transfer branch, where the route needs qualifying, and null where it does not.
        /// </summary>
        private static string CaptionText(PartFDoorTransferData partFDoorTransferData)
        {
            return partFDoorTransferData.OpeningStatus switch
            {
                Enums.PartFTransferOpeningStatus.MissingTransferOpening => "no modelled transfer opening",
                Enums.PartFTransferOpeningStatus.AmbiguousRoute => "route not fixed by the dwelling's topology",
                _ => null,
            };
        }

        private static string TransferText(PartFDoorTransferData partFDoorTransferData, PartFOperatingMode partFOperatingMode)
        {
            if (partFOperatingMode == PartFOperatingMode.MeasuredCommissioning)
            {
                //Appendix C Part 3 records fan and terminal rates, not door flows, so there is no measured
                //transfer air to show and none is invented.
                return "not measured";
            }

            double? flow = Rate(partFDoorTransferData, partFOperatingMode);

            return flow is null ? "not calculated" : string.Format("{0} l/s", Number(flow.Value));
        }

        /// <summary>
        /// The calculated transfer routing [l/s] through one internal opening at one operating condition,
        /// or null where none was calculated.
        /// <para>
        /// Public so that every surface showing this number - the text schematic, the floor plan overlay,
        /// the internal doors grid - reads it from the same place. Two readers of the same route must not
        /// be able to disagree about its value.
        /// </para>
        /// <para>
        /// It is SAM's airflow-network result, not an Approved Document F requirement: paragraph 1.25
        /// requires a free area through an internal door and prescribes no flow rate for one.
        /// </para>
        /// </summary>
        public static double? Rate(PartFDoorTransferData partFDoorTransferData, PartFOperatingMode partFOperatingMode)
        {
            return partFOperatingMode switch
            {
                PartFOperatingMode.ContinuousDesign => partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps,
                PartFOperatingMode.HighBoost => partFDoorTransferData.HighTransferFlowRate_Lps,
                PartFOperatingMode.Setback => partFDoorTransferData.SetbackTransferFlowRate_Lps,
                _ => null,
            };
        }

        /// <summary>The rate of one terminal at one operating condition, or null where it has none.</summary>
        public static double? Rate(PartFVentilationTerminalRequirement partFVentilationTerminalRequirement, PartFOperatingMode partFOperatingMode)
        {
            return partFOperatingMode switch
            {
                PartFOperatingMode.ContinuousDesign => partFVentilationTerminalRequirement.ContinuousDesignFlowRate_Lps,
                PartFOperatingMode.HighBoost => partFVentilationTerminalRequirement.HighFlowRate_Lps,
                PartFOperatingMode.Setback => partFVentilationTerminalRequirement.SetbackFlowRate_Lps,
                PartFOperatingMode.MeasuredCommissioning => partFVentilationTerminalRequirement.MeasuredContinuousFlowRate_Lps,
                _ => null,
            };
        }

        private static string RoleText(PartFTerminalRole partFTerminalRole)
        {
            return partFTerminalRole switch
            {
                PartFTerminalRole.Supply => "Supply",
                PartFTerminalRole.GeneralExtract => "General extract",
                PartFTerminalRole.LocalKitchenExtract => "Local kitchen extract",
                _ => "Terminal",
            };
        }

        /// <summary>A signed rate, using the minus sign rather than a hyphen for air leaving a space.</summary>
        public static string Signed(double value_Lps)
        {
            return value_Lps < 0
                ? string.Format("{0}{1} l/s", Minus, Number(-value_Lps))
                : string.Format("+{0} l/s", Number(value_Lps));
        }

        /// <summary>
        /// A rate as it appears in the diagram. Invariant culture throughout, so the decimal separator of
        /// the machine that produced a report can never change the numbers a regression test compares.
        /// </summary>
        public static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
