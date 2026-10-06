// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Assesses purge ventilation in one habitable room against Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England) paragraphs 1.26 to 1.31 and Table 1.4 (page 11).
    /// <para>
    /// The requirement is calculable: four air changes per hour on the room volume (paragraph 1.27), and
    /// a minimum total area of openings taken from Table 1.4 as a fraction of the room's floor area.
    /// Whether the room MEETS it generally is not, because an openable area depends on which lights of a
    /// window actually open and how far, and that is a product property rather than analytical geometry.
    /// </para>
    /// <para>
    /// The window area the model does carry is therefore reported as context and never taken as the
    /// openable area. Table 1.4 is about the area of the OPENING; a fixed light adds to the window area
    /// and opens nothing.
    /// </para>
    /// </summary>
    public static class PartFPurgeAssessor
    {
        /// <summary>Paragraph reference used on every purge record.</summary>
        public const string SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraphs 1.26 to 1.31 and Table 1.4 (page 11)";

        /// <summary>
        /// Floating-point tolerance for comparing two areas in m2. <b>Not <see cref="PartFAirflowNetwork.Tolerance_Lps"/></b>,
        /// which is a flow-rate tolerance in l/s: reusing it here compared an area difference against a
        /// number named and sized for a different quantity, which happened to be harmless only because that
        /// constant's value is negligible for either unit. Named and owned by this class so a future change
        /// to the airflow tolerance cannot silently move this comparison's meaning with it.
        /// </summary>
        private const double Tolerance_M2 = 1e-6;

        /// <summary>
        /// The Part O interaction of paragraph 0.21 (page 4), reported on every habitable room so the
        /// higher of the two standards is applied knowingly.
        /// </summary>
        public const string PartOInteractionNote = "Approved Document F paragraph 0.21 (page 4): for domestic-type buildings, Approved Document O may require a higher purge ventilation standard than this, to remove excess heat. Where it does, the higher of the two standards applies. The Part O requirement is assessed separately and is not included in the figures here.";

        /// <summary>
        /// Assesses one room.
        /// </summary>
        /// <param name="space">The room.</param>
        /// <param name="adjacencyCluster">The model, read for the room's external openings.</param>
        /// <param name="isHabitable">
        /// True where paragraph 1.26 requires purge ventilation here, i.e. the room is habitable.
        /// </param>
        /// <param name="partFPurgeVentilationData_Existing">
        /// The record from a previous run, so the engineering inputs a person supplied survive this one.
        /// </param>
        public static PartFPurgeVentilationData Assess(
            Space space,
            AdjacencyCluster adjacencyCluster,
            bool isHabitable,
            PartFPurgeVentilationData partFPurgeVentilationData_Existing = null)
        {
            if (space is null)
            {
                return null;
            }

            PartFPurgeVentilationData result = new(space.Name)
            {
                SpaceGuid = space.Guid,
                SpaceName = space.Name,
                IsRequired = isHabitable,
                SourceReference = SourceReference,
                PartOInteractionNote = PartOInteractionNote,
            };

            result.TakeInputsFrom(partFPurgeVentilationData_Existing);

            double volume_M3 = space.GetValue<double>(SpaceParameter.Volume);
            double area_M2 = space.GetValue<double>(SpaceParameter.Area);

            result.RoomVolume_M3 = volume_M3 > 0 ? volume_M3 : null;
            result.RoomFloorArea_M2 = area_M2 > 0 ? area_M2 : null;

            //Paragraph 1.27: at least four air changes per hour. Air changes per hour on a volume in m3
            //give m3/h, so the conversion to l/s is x1000/3600.
            if (result.RoomVolume_M3 is not null)
            {
                result.RequiredPurgeRate_Lps = result.RequiredAirChangesPerHour_Value * result.RoomVolume_M3.Value * 1000 / 3600;
            }

            //Table 1.4: the fraction depends on the opening type and angle, which are product properties.
            //With no opening type recorded there is no row and therefore no required area - reported as
            //unknown rather than defaulted to the more permissive 1/20.
            double? fraction = PartFPurgeVentilationData.Table1_4AreaFraction(result.OpeningType);
            if (fraction is not null && result.RoomFloorArea_M2 is not null)
            {
                result.RequiredOpeningArea_M2 = fraction.Value * result.RoomFloorArea_M2.Value;
            }

            ReadGeometry(space, adjacencyCluster, result);

            AssessStatus(result);

            return result;
        }

        /// <summary>
        /// Reads what the model can actually say: whether the room has any aperture on an external
        /// element, and how much window area that amounts to.
        /// </summary>
        private static void ReadGeometry(Space space, AdjacencyCluster adjacencyCluster, PartFPurgeVentilationData partFPurgeVentilationData)
        {
            if (adjacencyCluster is null)
            {
                return;
            }

            double area_M2 = 0;
            bool hasExternalOpening = false;

            foreach (Panel panel in adjacencyCluster.GetPanels(space) ?? [])
            {
                if (panel is null)
                {
                    continue;
                }

                //One adjacent space means the element separates the room from outside. The same rule the
                //airflow network uses, so "external" means one thing across the whole assessment.
                List<Space> spaces_Panel = adjacencyCluster.GetSpaces(panel);
                if (spaces_Panel is null || spaces_Panel.Count != 1)
                {
                    continue;
                }

                foreach (Aperture aperture in panel.Apertures ?? [])
                {
                    if (aperture is null)
                    {
                        continue;
                    }

                    hasExternalOpening = true;

                    double area_Aperture = aperture.GetArea();
                    if (!double.IsNaN(area_Aperture) && !double.IsInfinity(area_Aperture) && area_Aperture > 0)
                    {
                        area_M2 += area_Aperture;
                    }
                }
            }

            partFPurgeVentilationData.HasExternalOpening = hasExternalOpening;
            partFPurgeVentilationData.ExternalApertureArea_M2 = hasExternalOpening ? area_M2 : null;

            //Paragraph 1.27 requires the purge to be directly to the outside. An opening on an external
            //element gives that route; a mechanical purge system provides it by other means.
            partFPurgeVentilationData.IsPurgeRouteDirectlyOutside =
                hasExternalOpening || partFPurgeVentilationData.PurgeMethod == PartFPurgeMethod.MechanicalExtract;
        }

        private static void AssessStatus(PartFPurgeVentilationData partFPurgeVentilationData)
        {
            if (!partFPurgeVentilationData.IsRequired)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.NotApplicable;
                partFPurgeVentilationData.Diagnostic = "Not a habitable room, so paragraph 1.26 does not require purge ventilation here.";
                return;
            }

            if (partFPurgeVentilationData.RoomVolume_M3 is null)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = "The room has no volume, so the four air changes per hour of paragraph 1.27 could not be applied. Check that the space carries a Volume parameter in m3.";
                return;
            }

            if (partFPurgeVentilationData.PurgeMethod == PartFPurgeMethod.MechanicalExtract)
            {
                AssessMechanical(partFPurgeVentilationData);
                return;
            }

            //An unrecorded method with an external opening present is treated as the openings route of
            //paragraph 1.28a, because that is what the geometry shows; the assessment below still refuses
            //to pass it without an opening area.
            if (!partFPurgeVentilationData.IsPurgeRouteDirectlyOutside)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.EngineeringReviewRequired;
                partFPurgeVentilationData.Diagnostic = string.Format("This habitable room has no aperture on an external element, so it cannot purge directly to the outside as paragraph 1.27 requires. Either provide a mechanical purge system under paragraph 1.28b, or follow paragraphs 1.42 to 1.44 to ventilate it through an adjoining habitable room or conservatory, which needs a permanent opening of at least 1/20 of the two rooms' combined floor area. Required purge rate: {0:0.##} l/s.", partFPurgeVentilationData.RequiredPurgeRate_Lps);
                return;
            }

            if (partFPurgeVentilationData.OpeningType == PartFPurgeOpeningType.HingedOrPivotUnder15Degrees)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.Fail;
                partFPurgeVentilationData.Diagnostic = "Paragraph 1.31: hinged or pivot windows with an opening angle of less than 15 degrees are not suitable for purge ventilation, so this opening provides none.";
                return;
            }

            if (partFPurgeVentilationData.OpeningType == PartFPurgeOpeningType.Undefined)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = string.Format("The opening type and angle are not recorded, so the Table 1.4 minimum opening area could not be selected: a hinged or pivot window opening 15 to 30 degrees needs 1/10 of the floor area, and one opening 30 degrees or more, an opening sash window or an external door needs 1/20. Required purge rate: {0:0.##} l/s over a room volume of {1:0.##} m3. The model shows {2} of window area on external elements, which is the area of the windows and not the area they open to, so it is not used as the opening area. Record the opening type, the opening angle and the openable area.", partFPurgeVentilationData.RequiredPurgeRate_Lps, partFPurgeVentilationData.RoomVolume_M3, partFPurgeVentilationData.ExternalApertureArea_M2 is null ? "no" : string.Format("{0:0.##} m2", partFPurgeVentilationData.ExternalApertureArea_M2));
                return;
            }

            if (partFPurgeVentilationData.RequiredOpeningArea_M2 is null)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = "The room has no floor area, so the Table 1.4 minimum opening area could not be calculated. Check that the space carries an Area parameter in m2.";
                return;
            }

            double? provided = partFPurgeVentilationData.ProvidedOpeningArea_M2();
            if (provided is null)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = string.Format("Table 1.4 requires a minimum total opening area of {0:0.###} m2 for this room, and no openable window area or external door opening area has been recorded. The model shows {1} of window area on external elements, which is the area of the windows and not the area they open to, so it is not used as the opening area. Enter the openable area.", partFPurgeVentilationData.RequiredOpeningArea_M2, partFPurgeVentilationData.ExternalApertureArea_M2 is null ? "no" : string.Format("{0:0.##} m2", partFPurgeVentilationData.ExternalApertureArea_M2));
                return;
            }

            //A non-finite recorded area (NaN or infinity - an unrecorded optional double that arrived as NaN,
            //or a hand-edited file) must not fall through the comparisons below: NaN fails every comparison
            //and would have passed the room, which is absence of evidence reading as compliance.
            if (double.IsNaN(provided.Value) || double.IsInfinity(provided.Value))
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = "The recorded opening area is not a finite number, so Table 1.4 could not be assessed. Enter the openable area in m2.";
                return;
            }

            if (provided.Value + Tolerance_M2 < partFPurgeVentilationData.RequiredOpeningArea_M2.Value)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.Fail;
                partFPurgeVentilationData.Diagnostic = string.Format("The opening area provided is {0:0.###} m2 against the {1:0.###} m2 required by Table 1.4 for this opening type. Paragraph 1.30 allows a smaller opening only where expert advice shows that four air changes per hour are still achieved.", provided.Value, partFPurgeVentilationData.RequiredOpeningArea_M2.Value);
                return;
            }

            partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.Pass;
            partFPurgeVentilationData.Diagnostic = string.Format("The opening area provided is {0:0.###} m2 against the {1:0.###} m2 required by Table 1.4 for this opening type, for a purge rate of {2:0.##} l/s over a room volume of {3:0.##} m3.", provided.Value, partFPurgeVentilationData.RequiredOpeningArea_M2.Value, partFPurgeVentilationData.RequiredPurgeRate_Lps, partFPurgeVentilationData.RoomVolume_M3);
        }

        private static void AssessMechanical(PartFPurgeVentilationData partFPurgeVentilationData)
        {
            if (partFPurgeVentilationData.MechanicalPurgeCapacity_Lps is null)
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                partFPurgeVentilationData.Diagnostic = string.Format("Purge ventilation is recorded as mechanical under paragraph 1.28b, but no capacity has been entered. Paragraph 1.27 requires at least four air changes per hour directly to the outside, which is {0:0.##} l/s for this room.", partFPurgeVentilationData.RequiredPurgeRate_Lps);
                return;
            }

            if (partFPurgeVentilationData.MechanicalPurgeCapacity_Lps.Value + PartFAirflowNetwork.Tolerance_Lps < (partFPurgeVentilationData.RequiredPurgeRate_Lps ?? 0))
            {
                partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.Fail;
                partFPurgeVentilationData.Diagnostic = string.Format("The mechanical purge capacity of {0:0.##} l/s is below the {1:0.##} l/s that four air changes per hour require in this room (paragraph 1.27).", partFPurgeVentilationData.MechanicalPurgeCapacity_Lps.Value, partFPurgeVentilationData.RequiredPurgeRate_Lps);
                return;
            }

            partFPurgeVentilationData.ComplianceStatus = PartFComplianceStatus.Pass;
            partFPurgeVentilationData.Diagnostic = string.Format("Mechanical purge under paragraph 1.28b provides {0:0.##} l/s against the {1:0.##} l/s required by the four air changes per hour of paragraph 1.27.", partFPurgeVentilationData.MechanicalPurgeCapacity_Lps.Value, partFPurgeVentilationData.RequiredPurgeRate_Lps);
        }
    }
}
