// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Turns a solved <see cref="PartFAirflowNetwork"/> into the dwelling's internal transfer air
    /// schedule: one record per internal route, carrying both the paragraph 1.25 free area requirement
    /// and the transfer flow the network put through it.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England) paragraph 1.25
    /// (page 10) applies to internal doors as such, not only to the doors the flow solver happens to
    /// load. Every internal route within the dwelling therefore gets a record and a requirement, whether
    /// the calculated transfer flow through it is 30 l/s or nothing at all.
    /// </para>
    /// </summary>
    public static class PartFTransferPathBuilder
    {
        /// <summary>
        /// Paragraph reference used on every transfer record.
        /// </summary>
        public const string SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.25 (page 10)";

        /// <summary>
        /// Builds the transfer air schedule of one dwelling.
        /// </summary>
        /// <param name="partFAirflowNetwork">The dwelling's internal airflow network.</param>
        /// <param name="dictionary_Continuous">Connection flows [l/s] at the continuous design condition.</param>
        /// <param name="dictionary_High">Connection flows [l/s] with the system at its high rate.</param>
        /// <param name="setbackFlowRateFactor">Factor applied to the continuous flows to obtain setback flows.</param>
        /// <param name="dwellingName">Name of the dwelling, or null in single dwelling mode.</param>
        /// <param name="dictionary_Existing">
        /// Transfer records already on the model's door apertures, so the engineering inputs a person
        /// supplied - the provided undercut, the provided free area, the transfer device type, any
        /// transfer flow override - survive this recalculation.
        /// </param>
        public static List<PartFDoorTransferData> Build(
            PartFAirflowNetwork partFAirflowNetwork,
            Dictionary<(Guid, Guid), double> dictionary_Continuous,
            Dictionary<(Guid, Guid), double> dictionary_High,
            double setbackFlowRateFactor,
            string dwellingName,
            Dictionary<Guid, PartFDoorTransferData> dictionary_Existing = null)
        {
            List<PartFDoorTransferData> result = [];

            if (partFAirflowNetwork is null)
            {
                return result;
            }

            foreach ((Guid, Guid) connection in partFAirflowNetwork.Connections)
            {
                double flow_Continuous = Flow(dictionary_Continuous, connection);
                double flow_High = Flow(dictionary_High, connection);

                //The record is written in the direction the air actually moves at the continuous design
                //condition, so a schedule reads "Studio to Bathroom" rather than carrying a negative
                //number the reader has to interpret.
                //
                //Where nothing flows there is no direction to read, so the two spaces are ordered by name.
                //The connection's own canonical order is by guid, and a guid is assigned afresh every time
                //a model is built, so using it here would make an unloaded route read "Bathroom to WC" on
                //one run and "WC to Bathroom" on the next.
                bool reversed = System.Math.Abs(flow_Continuous) <= PartFAirflowNetwork.Tolerance_Lps
                    ? string.CompareOrdinal(partFAirflowNetwork.Name(connection.Item1), partFAirflowNetwork.Name(connection.Item2)) > 0
                    : flow_Continuous < 0;

                Guid guid_Upstream = reversed ? connection.Item2 : connection.Item1;
                Guid guid_Downstream = reversed ? connection.Item1 : connection.Item2;

                double flow_Continuous_Directed = System.Math.Abs(flow_Continuous);
                double flow_High_Directed = reversed ? -flow_High : flow_High;

                List<Aperture> apertures = partFAirflowNetwork.Apertures(connection);

                PartFTransferRouteStatus routeStatus = partFAirflowNetwork.IsUniquelyDetermined(guid_Upstream)
                    ? PartFTransferRouteStatus.UniquelyDetermined
                    : PartFTransferRouteStatus.AllocationStrategy;

                if (apertures.Count == 0)
                {
                    result.Add(Create(
                        null,
                        partFAirflowNetwork,
                        guid_Upstream,
                        guid_Downstream,
                        flow_Continuous_Directed,
                        flow_High_Directed,
                        setbackFlowRateFactor,
                        routeStatus,
                        dwellingName,
                        1,
                        dictionary_Existing));

                    continue;
                }

                //Several doors between the same two rooms carry the route's air between them. Approved
                //Document F says nothing about how it divides, so an equal split is used and every one of
                //them is reported as Ambiguous - each still has to provide the paragraph 1.25 free area in
                //its own right, which is the part that actually matters.
                PartFTransferRouteStatus routeStatus_Aperture = apertures.Count > 1 ? PartFTransferRouteStatus.Ambiguous : routeStatus;

                foreach (Aperture aperture in apertures)
                {
                    result.Add(Create(
                        aperture,
                        partFAirflowNetwork,
                        guid_Upstream,
                        guid_Downstream,
                        flow_Continuous_Directed,
                        flow_High_Directed,
                        setbackFlowRateFactor,
                        routeStatus_Aperture,
                        dwellingName,
                        apertures.Count,
                        dictionary_Existing));
                }
            }

            return [.. result
                .OrderBy(x => x.UpstreamSpaceName, StringComparer.Ordinal)
                .ThenBy(x => x.DownstreamSpaceName, StringComparer.Ordinal)
                .ThenBy(x => x.Name, StringComparer.Ordinal)];
        }

        private static double Flow(Dictionary<(Guid, Guid), double> dictionary, (Guid, Guid) connection)
        {
            return dictionary is not null && dictionary.TryGetValue(connection, out double result) ? result : 0;
        }

        private static PartFDoorTransferData Create(
            Aperture aperture,
            PartFAirflowNetwork partFAirflowNetwork,
            Guid guid_Upstream,
            Guid guid_Downstream,
            double flow_Continuous_Lps,
            double flow_High_Lps,
            double setbackFlowRateFactor,
            PartFTransferRouteStatus routeStatus,
            string dwellingName,
            int apertureCount,
            Dictionary<Guid, PartFDoorTransferData> dictionary_Existing)
        {
            string name_Upstream = partFAirflowNetwork.Name(guid_Upstream);
            string name_Downstream = partFAirflowNetwork.Name(guid_Downstream);

            string name = aperture is null || string.IsNullOrWhiteSpace(aperture.Name)
                ? string.Format("{0} to {1}", name_Upstream, name_Downstream)
                : aperture.Name;

            PartFDoorTransferData result = new(name)
            {
                ApertureGuid = aperture?.Guid ?? Guid.Empty,
                UpstreamSpaceGuid = guid_Upstream,
                DownstreamSpaceGuid = guid_Downstream,
                UpstreamSpaceName = name_Upstream,
                DownstreamSpaceName = name_Downstream,
                DwellingName = dwellingName,

                //Both ends are inside one dwelling by construction: the network never creates a connection
                //to a space outside the dwelling being sized.
                IsInternalDwellingDoor = true,
                RequiresTransferAirPath = true,
                IsDoorRepresented = aperture is not null,

                MinimumRequiredFreeArea_mm2 = PartFDoorTransferData.NominalEquivalentFreeArea_mm2,
                RequiredUndercutHeightFinished_mm = PartFDoorTransferData.ReferenceUndercutHeight_mm,
                RequiredUndercutHeightBeforeFloorFinish_mm = PartFDoorTransferData.UndercutHeightBeforeFloorFinish_mm,

                ContinuousDesignTransferFlowRate_Lps = flow_Continuous_Lps / apertureCount,
                HighTransferFlowRate_Lps = flow_High_Lps / apertureCount,
                SetbackTransferFlowRate_Lps = flow_Continuous_Lps / apertureCount * setbackFlowRateFactor,

                RouteStatus = routeStatus,
                SourceReference = SourceReference,
                ClearDoorWidth_mm = aperture is null ? null : PartFAirflowNetwork.ClearDoorWidth_mm(aperture),
            };

            result.CalculationSource = CalculationSource(routeStatus, apertureCount);

            if (aperture is not null && dictionary_Existing is not null && dictionary_Existing.TryGetValue(aperture.Guid, out PartFDoorTransferData partFDoorTransferData_Existing))
            {
                result.TakeInputsFrom(partFDoorTransferData_Existing);
            }

            //Applied after the inputs are taken, so an override entered by the engineer wins over the
            //calculated allocation, and the setback and high rates follow it rather than contradicting it.
            if (result.TransferFlowRateOverride_Lps is not null)
            {
                double flow_Override = result.TransferFlowRateOverride_Lps.Value;

                double ratio = flow_Continuous_Lps / apertureCount;
                ratio = System.Math.Abs(ratio) <= PartFAirflowNetwork.Tolerance_Lps ? 1 : flow_Override / ratio;

                result.HighTransferFlowRate_Lps = (result.HighTransferFlowRate_Lps ?? 0) * ratio;
                result.ContinuousDesignTransferFlowRate_Lps = flow_Override;
                result.SetbackTransferFlowRate_Lps = flow_Override * setbackFlowRateFactor;
                result.RouteStatus = PartFTransferRouteStatus.UserOverridden;
                result.CalculationSource = "Transfer flow rate entered by the engineer, replacing the calculated allocation.";
            }

            Assess(result);

            return result;
        }

        private static string CalculationSource(PartFTransferRouteStatus partFTransferRouteStatus, int apertureCount)
        {
            string result = partFTransferRouteStatus switch
            {
                PartFTransferRouteStatus.UniquelyDetermined =>
                    "Fixed by conservation of air flow: the dwelling's internal connections form a tree, so only one set of transfer flows is possible and no engineering choice was involved.",

                PartFTransferRouteStatus.AllocationStrategy =>
                    "Calculated by the deterministic allocation strategy: air is routed from each net-supply space to each net-extract space in proportion to what each has to give and take, along the shortest connected path. The dwelling's connections contain a loop, so more than one valid split exists and Approved Document F does not choose between them. The total is correct; the split may be overridden.",

                PartFTransferRouteStatus.Ambiguous =>
                    "Calculated by the deterministic allocation strategy and then divided equally between the doors on this route. Approved Document F does not say how air divides between parallel doors between the same two rooms, so this split is a design decision and may be overridden.",

                PartFTransferRouteStatus.NotCalculable =>
                    "No transfer flow could be established for this route.",

                _ => null,
            };

            if (apertureCount > 1 && partFTransferRouteStatus != PartFTransferRouteStatus.Ambiguous)
            {
                result += string.Format(" Divided equally between the {0} doors on this route.", apertureCount);
            }

            return result;
        }

        /// <summary>
        /// Judges one route against paragraph 1.25. The free area is what the paragraph requires; the two
        /// undercut heights are the datum it is measured from, so a provided area at or above 7,600mm2 is
        /// the condition, and an undercut is converted to an area before being judged.
        /// <para>
        /// Absence of evidence is never a pass. A door with no recorded undercut is
        /// <see cref="PartFComplianceStatus.CannotBeDetermined"/>, not compliant, because an analytical
        /// model does not represent the gap under a door leaf and a door's modelled height is not evidence
        /// of one.
        /// </para>
        /// </summary>
        public static void Assess(PartFDoorTransferData partFDoorTransferData)
        {
            if (partFDoorTransferData is null)
            {
                return;
            }

            if (!partFDoorTransferData.IsInternalDwellingDoor)
            {
                partFDoorTransferData.ComplianceStatus = PartFComplianceStatus.NotApplicable;
                partFDoorTransferData.Diagnostic = "Not an internal door within one dwelling, so paragraph 1.25 does not apply to it. External doors, dwelling entrance doors onto communal areas and doors between separate dwellings carry no internal transfer air requirement.";
                return;
            }

            double required = partFDoorTransferData.MinimumRequiredFreeArea_mm2 ?? PartFDoorTransferData.NominalEquivalentFreeArea_mm2;

            double? provided = partFDoorTransferData.EffectiveProvidedFreeArea_mm2();

            if (provided is null)
            {
                partFDoorTransferData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;

                partFDoorTransferData.Diagnostic = partFDoorTransferData.IsDoorRepresented
                    ? string.Format("No undercut or free area is recorded for this door, so paragraph 1.25 cannot be assessed. The requirement is a minimum free area of {0:0.##} mm2, equivalent to a {1:0.##} mm undercut in a {2:0.##} mm wide door, achieved as {1:0.##} mm above a fitted floor finish or {3:0.##} mm above an unfinished floor surface. Enter the provided undercut or free area, or record a transfer grille or permanent opening.", required, PartFDoorTransferData.ReferenceUndercutHeight_mm, PartFDoorTransferData.ReferenceDoorWidth_mm, PartFDoorTransferData.UndercutHeightBeforeFloorFinish_mm)
                    : string.Format("These two rooms are adjacent and air has to move between them, but no door or other transfer opening is modelled on the separating element, so there is nothing to assess against paragraph 1.25. The requirement is a minimum free area of {0:0.##} mm2. Model the door, or record the transfer provision explicitly.", required);

                return;
            }

            if (partFDoorTransferData.TransferDeviceType == PartFTransferDeviceType.NotRepresented && provided.Value > 0)
            {
                //A provided area was entered but nothing says what provides it. Recorded as an undercut,
                //which is the arrangement paragraph 1.25 describes, and said so in the diagnostic.
                partFDoorTransferData.TransferDeviceType = PartFTransferDeviceType.DoorUndercut;
            }

            string basis = partFDoorTransferData.ProvidedFreeArea_mm2 is not null
                ? "the free area entered"
                : partFDoorTransferData.ClearDoorWidth_mm is not null
                    ? string.Format("a {0:0.##} mm undercut across the {1:0.##} mm modelled door width", partFDoorTransferData.ProvidedUndercutHeight_mm, partFDoorTransferData.ClearDoorWidth_mm)
                    : string.Format("a {0:0.##} mm undercut across the {1:0.##} mm reference door width, no door width being known", partFDoorTransferData.ProvidedUndercutHeight_mm, PartFDoorTransferData.ReferenceDoorWidth_mm);

            if (provided.Value + PartFAirflowNetwork.Tolerance_Lps < required)
            {
                partFDoorTransferData.ComplianceStatus = PartFComplianceStatus.Fail;
                partFDoorTransferData.Diagnostic = string.Format("The transfer free area provided is {0:0.##} mm2, from {1}, against the {2:0.##} mm2 required by paragraph 1.25. Increase the undercut, widen the door, or add a transfer grille or permanent opening.", provided.Value, basis, required);
                return;
            }

            //The area is met. Where the arrangement is an undercut and the floor finish state is known,
            //the paragraph 1.25a and 1.25b datum is checked as well, because a shallow undercut in a very
            //wide door can reach the area while still not being the arrangement the paragraph describes.
            if (partFDoorTransferData.TransferDeviceType == PartFTransferDeviceType.DoorUndercut
                && partFDoorTransferData.ProvidedUndercutHeight_mm is not null
                && partFDoorTransferData.IsFloorFinishFitted is not null)
            {
                double required_Undercut = partFDoorTransferData.IsFloorFinishFitted.Value
                    ? partFDoorTransferData.RequiredUndercutHeightFinished_mm ?? PartFDoorTransferData.ReferenceUndercutHeight_mm
                    : partFDoorTransferData.RequiredUndercutHeightBeforeFloorFinish_mm ?? PartFDoorTransferData.UndercutHeightBeforeFloorFinish_mm;

                if (partFDoorTransferData.ProvidedUndercutHeight_mm.Value + PartFAirflowNetwork.Tolerance_Lps < required_Undercut)
                {
                    partFDoorTransferData.ComplianceStatus = PartFComplianceStatus.Fail;
                    partFDoorTransferData.Diagnostic = string.Format("The transfer free area of {0:0.##} mm2 meets the {1:0.##} mm2 required by paragraph 1.25, but the {2:0.##} mm undercut is below the {3:0.##} mm required {4}.", provided.Value, required, partFDoorTransferData.ProvidedUndercutHeight_mm.Value, required_Undercut, partFDoorTransferData.IsFloorFinishFitted.Value ? "above a fitted floor finish (paragraph 1.25a)" : "above an unfinished floor surface (paragraph 1.25b)");
                    return;
                }
            }

            partFDoorTransferData.ComplianceStatus = PartFComplianceStatus.Pass;
            partFDoorTransferData.Diagnostic = string.Format("The transfer free area provided is {0:0.##} mm2, from {1}, against the {2:0.##} mm2 required by paragraph 1.25.", provided.Value, basis, required);
        }
    }
}
