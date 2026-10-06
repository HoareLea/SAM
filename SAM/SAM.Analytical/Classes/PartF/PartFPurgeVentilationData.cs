// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The Approved Document F purge ventilation requirement for one habitable room, and what the model
    /// can say about whether the room provides it.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England):
    /// </para>
    /// <list type="bullet">
    /// <item>paragraph 1.26 (page 11): a system for purge ventilation should be provided in each habitable
    /// room;</item>
    /// <item>paragraph 1.27 (page 11): purge ventilation should be capable of extracting at least four air
    /// changes per hour per room directly to the outside;</item>
    /// <item>paragraph 1.28 (page 11): delivered through openings (windows or doors) or a mechanical
    /// extract ventilation system;</item>
    /// <item>paragraph 1.29 and Table 1.4 (page 11): where delivered through openings, minimum opening
    /// areas as a fraction of the room's floor area, set by the opening type and angle;</item>
    /// <item>paragraph 1.31 (page 11): hinged or pivot windows opening less than 15 degrees are not
    /// suitable for purge ventilation.</item>
    /// </list>
    /// <para>
    /// The requirement is calculable from the room's volume and floor area. Whether the room MEETS it
    /// generally is not: an openable area depends on which lights of a window actually open and how far,
    /// which is a product property and not analytical geometry. The window area the model does carry is
    /// therefore reported as context under <see cref="ExternalApertureArea_M2"/> and never taken as the
    /// openable area - Table 1.4 is about the area of the OPENING, not the area of the window.
    /// </para>
    /// <para>
    /// Paragraph 0.21 (page 4): for domestic-type buildings Part O may require a higher purge standard
    /// than this document, and where it does the higher of the two applies. That interaction is reported,
    /// not calculated here.
    /// </para>
    /// </summary>
    public class PartFPurgeVentilationData : SAMObject
    {
        /// <summary>
        /// Air changes per hour required by paragraph 1.27 (page 11).
        /// </summary>
        public const double RequiredAirChangesPerHour = 4;

        public PartFPurgeVentilationData()
        {
        }

        public PartFPurgeVentilationData(string name)
            : base(name)
        {
        }

        public PartFPurgeVentilationData(PartFPurgeVentilationData partFPurgeVentilationData)
            : base(partFPurgeVentilationData)
        {
            if (partFPurgeVentilationData is not null)
            {
                SpaceGuid = partFPurgeVentilationData.SpaceGuid;
                SpaceName = partFPurgeVentilationData.SpaceName;
                IsRequired = partFPurgeVentilationData.IsRequired;
                RoomVolume_M3 = partFPurgeVentilationData.RoomVolume_M3;
                RoomFloorArea_M2 = partFPurgeVentilationData.RoomFloorArea_M2;
                RequiredAirChangesPerHour_Value = partFPurgeVentilationData.RequiredAirChangesPerHour_Value;
                RequiredPurgeRate_Lps = partFPurgeVentilationData.RequiredPurgeRate_Lps;
                RequiredOpeningArea_M2 = partFPurgeVentilationData.RequiredOpeningArea_M2;
                ExternalApertureArea_M2 = partFPurgeVentilationData.ExternalApertureArea_M2;
                HasExternalOpening = partFPurgeVentilationData.HasExternalOpening;
                IsPurgeRouteDirectlyOutside = partFPurgeVentilationData.IsPurgeRouteDirectlyOutside;
                PurgeMethod = partFPurgeVentilationData.PurgeMethod;
                OpeningType = partFPurgeVentilationData.OpeningType;
                OpeningAngle_Degrees = partFPurgeVentilationData.OpeningAngle_Degrees;
                OpenableWindowArea_M2 = partFPurgeVentilationData.OpenableWindowArea_M2;
                ExternalDoorOpeningArea_M2 = partFPurgeVentilationData.ExternalDoorOpeningArea_M2;
                MechanicalPurgeCapacity_Lps = partFPurgeVentilationData.MechanicalPurgeCapacity_Lps;
                ComplianceStatus = partFPurgeVentilationData.ComplianceStatus;
                SourceReference = partFPurgeVentilationData.SourceReference;
                Diagnostic = partFPurgeVentilationData.Diagnostic;
                PartOInteractionNote = partFPurgeVentilationData.PartOInteractionNote;
            }
        }

        public PartFPurgeVentilationData(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        // ------------------------------------------------------------------
        // Identity and requirement (derived - recalculated every run)
        // ------------------------------------------------------------------

        /// <summary>The habitable room this requirement belongs to.</summary>
        public Guid SpaceGuid { get; set; } = Guid.Empty;

        /// <summary>Name of that room, held so a schedule reads without resolving guids.</summary>
        public string SpaceName { get; set; }

        /// <summary>
        /// True where paragraph 1.26 requires purge ventilation in this room, i.e. it is a habitable room.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>Volume [m3] the four air changes per hour are applied to.</summary>
        public double? RoomVolume_M3 { get; set; }

        /// <summary>Floor area [m2] the Table 1.4 opening area fraction is applied to.</summary>
        public double? RoomFloorArea_M2 { get; set; }

        /// <summary>
        /// Air changes per hour required, normally <see cref="RequiredAirChangesPerHour"/>. Held on the
        /// record so a result carries its own basis.
        /// </summary>
        public double RequiredAirChangesPerHour_Value { get; set; } = RequiredAirChangesPerHour;

        /// <summary>
        /// Purge rate [l/s] required by paragraph 1.27, i.e. the air changes per hour applied to the room
        /// volume and converted from m3/h to l/s.
        /// </summary>
        public double? RequiredPurgeRate_Lps { get; set; }

        /// <summary>
        /// Minimum total area of openings [m2] required by Table 1.4 for the recorded
        /// <see cref="OpeningType"/>. Null where the opening type is not known, because Table 1.4 selects
        /// the fraction by opening type and angle and neither can be guessed at.
        /// </summary>
        public double? RequiredOpeningArea_M2 { get; set; }

        /// <summary>
        /// Total area [m2] of the room's apertures on external elements, read from the model geometry.
        /// <para>
        /// Context only. This is the area of the windows, not the area of the openings, so it is never
        /// used as the openable area for Table 1.4. A fixed light contributes to it and opens nothing.
        /// </para>
        /// </summary>
        public double? ExternalApertureArea_M2 { get; set; }

        /// <summary>
        /// True where the room has at least one aperture on an external element, so a purge route to the
        /// outside is at least geometrically possible.
        /// </summary>
        public bool HasExternalOpening { get; set; }

        /// <summary>
        /// True where the room can purge directly to the outside as paragraph 1.27 requires. False where
        /// the room is internal, in which case paragraphs 1.42 to 1.44 (ventilation of a habitable room
        /// through another room) or a mechanical purge system applies.
        /// </summary>
        public bool IsPurgeRouteDirectlyOutside { get; set; }

        // ------------------------------------------------------------------
        // Engineering input (carried forward - never overwritten by the calculation)
        // ------------------------------------------------------------------

        /// <summary>How purge ventilation is provided, per paragraph 1.28.</summary>
        public PartFPurgeMethod PurgeMethod { get; set; } = PartFPurgeMethod.NotRepresented;

        /// <summary>
        /// The Table 1.4 opening type row that applies, which sets the required area fraction. An
        /// engineering input: the opening angle of a window is a product property, not model geometry.
        /// </summary>
        public PartFPurgeOpeningType OpeningType { get; set; } = PartFPurgeOpeningType.Undefined;

        /// <summary>
        /// Opening angle [degrees] of a hinged or pivot window, recorded for traceability alongside the
        /// Table 1.4 row it selects.
        /// </summary>
        public double? OpeningAngle_Degrees { get; set; }

        /// <summary>Openable window area [m2] provided. An engineering input.</summary>
        public double? OpenableWindowArea_M2 { get; set; }

        /// <summary>External door opening area [m2] provided. An engineering input.</summary>
        public double? ExternalDoorOpeningArea_M2 { get; set; }

        /// <summary>
        /// Capacity [l/s] of a mechanical purge system serving this room, where purge is delivered
        /// mechanically under paragraph 1.28b. An engineering input.
        /// </summary>
        public double? MechanicalPurgeCapacity_Lps { get; set; }

        // ------------------------------------------------------------------
        // Assessment
        // ------------------------------------------------------------------

        /// <summary>Outcome of assessing this room against paragraphs 1.26 to 1.31.</summary>
        public PartFComplianceStatus ComplianceStatus { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>The Approved Document paragraphs the requirement comes from.</summary>
        public string SourceReference { get; set; }

        /// <summary>Why the room reached that status, in the engineer's language.</summary>
        public string Diagnostic { get; set; }

        /// <summary>
        /// The Part O interaction from paragraph 0.21 (page 4), reported separately so the higher of the
        /// two standards is applied knowingly rather than the Part F figure being taken as final.
        /// </summary>
        public string PartOInteractionNote { get; set; }

        /// <summary>
        /// Total opening area [m2] the room actually provides: openable windows plus external doors.
        /// Null where neither has been entered.
        /// </summary>
        public double? ProvidedOpeningArea_M2()
        {
            if (OpenableWindowArea_M2 is null && ExternalDoorOpeningArea_M2 is null)
            {
                return null;
            }

            return (OpenableWindowArea_M2 ?? 0) + (ExternalDoorOpeningArea_M2 ?? 0);
        }

        /// <summary>
        /// Copies the engineering inputs from a previous record onto this one, so a recalculation keeps
        /// what only a person could have supplied.
        /// </summary>
        public void TakeInputsFrom(PartFPurgeVentilationData partFPurgeVentilationData)
        {
            if (partFPurgeVentilationData is null)
            {
                return;
            }

            if (partFPurgeVentilationData.PurgeMethod != PartFPurgeMethod.NotRepresented)
            {
                PurgeMethod = partFPurgeVentilationData.PurgeMethod;
            }

            if (partFPurgeVentilationData.OpeningType != PartFPurgeOpeningType.Undefined)
            {
                OpeningType = partFPurgeVentilationData.OpeningType;
            }

            OpeningAngle_Degrees = partFPurgeVentilationData.OpeningAngle_Degrees;
            OpenableWindowArea_M2 = partFPurgeVentilationData.OpenableWindowArea_M2;
            ExternalDoorOpeningArea_M2 = partFPurgeVentilationData.ExternalDoorOpeningArea_M2;
            MechanicalPurgeCapacity_Lps = partFPurgeVentilationData.MechanicalPurgeCapacity_Lps;
        }

        /// <summary>
        /// The fraction of the room's floor area that Table 1.4 (page 11) requires as a minimum total area
        /// of openings, or null where the opening type does not select a row.
        /// </summary>
        public static double? Table1_4AreaFraction(PartFPurgeOpeningType partFPurgeOpeningType)
        {
            return partFPurgeOpeningType switch
            {
                //"Hinged or pivot windows with an opening angle of 15 to 30 degrees: 1/10 of the floor
                //area of the room."
                PartFPurgeOpeningType.HingedOrPivot15To30Degrees => 1.0 / 10.0,

                //"Hinged or pivot windows with an opening angle of greater than or equal to 30 degrees",
                //"Opening sash windows" and "External doors": 1/20 of the floor area of the room.
                PartFPurgeOpeningType.HingedOrPivot30DegreesOrMore => 1.0 / 20.0,
                PartFPurgeOpeningType.OpeningSashWindow => 1.0 / 20.0,
                PartFPurgeOpeningType.ExternalDoor => 1.0 / 20.0,

                //Paragraph 1.31: an opening angle of less than 15 degrees is not suitable for purge
                //ventilation at all, so no area of it counts and Table 1.4 gives no row.
                _ => null,
            };
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            SpaceGuid = PartFJson.Guid(jsonObject, "SpaceGuid");
            SpaceName = PartFJson.String(jsonObject, "SpaceName");

            IsRequired = PartFJson.Boolean(jsonObject, "IsRequired");
            HasExternalOpening = PartFJson.Boolean(jsonObject, "HasExternalOpening");
            IsPurgeRouteDirectlyOutside = PartFJson.Boolean(jsonObject, "IsPurgeRouteDirectlyOutside");

            RoomVolume_M3 = PartFJson.NullableDouble(jsonObject, "RoomVolume_M3");
            RoomFloorArea_M2 = PartFJson.NullableDouble(jsonObject, "RoomFloorArea_M2");
            RequiredAirChangesPerHour_Value = PartFJson.NullableDouble(jsonObject, "RequiredAirChangesPerHour") ?? RequiredAirChangesPerHour;
            RequiredPurgeRate_Lps = PartFJson.NullableDouble(jsonObject, "RequiredPurgeRate_Lps");
            RequiredOpeningArea_M2 = PartFJson.NullableDouble(jsonObject, "RequiredOpeningArea_M2");
            ExternalApertureArea_M2 = PartFJson.NullableDouble(jsonObject, "ExternalApertureArea_M2");
            OpeningAngle_Degrees = PartFJson.NullableDouble(jsonObject, "OpeningAngle_Degrees");
            OpenableWindowArea_M2 = PartFJson.NullableDouble(jsonObject, "OpenableWindowArea_M2");
            ExternalDoorOpeningArea_M2 = PartFJson.NullableDouble(jsonObject, "ExternalDoorOpeningArea_M2");
            MechanicalPurgeCapacity_Lps = PartFJson.NullableDouble(jsonObject, "MechanicalPurgeCapacity_Lps");

            if (jsonObject.ContainsKey("PurgeMethod"))
            {
                PurgeMethod = Core.Query.Enum<PartFPurgeMethod>(PartFJson.String(jsonObject, "PurgeMethod"));
            }

            if (jsonObject.ContainsKey("OpeningType"))
            {
                OpeningType = Core.Query.Enum<PartFPurgeOpeningType>(PartFJson.String(jsonObject, "OpeningType"));
            }

            if (jsonObject.ContainsKey("ComplianceStatus"))
            {
                ComplianceStatus = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "ComplianceStatus"));
            }

            SourceReference = PartFJson.String(jsonObject, "SourceReference");
            Diagnostic = PartFJson.String(jsonObject, "Diagnostic");
            PartOInteractionNote = PartFJson.String(jsonObject, "PartOInteractionNote");

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            result["SpaceGuid"] = SpaceGuid.ToString();
            PartFJson.SetString(result, "SpaceName", SpaceName);

            result["IsRequired"] = IsRequired;
            result["HasExternalOpening"] = HasExternalOpening;
            result["IsPurgeRouteDirectlyOutside"] = IsPurgeRouteDirectlyOutside;

            PartFJson.SetNullableDouble(result, "RoomVolume_M3", RoomVolume_M3);
            PartFJson.SetNullableDouble(result, "RoomFloorArea_M2", RoomFloorArea_M2);
            PartFJson.SetNullableDouble(result, "RequiredAirChangesPerHour", RequiredAirChangesPerHour_Value);
            PartFJson.SetNullableDouble(result, "RequiredPurgeRate_Lps", RequiredPurgeRate_Lps);
            PartFJson.SetNullableDouble(result, "RequiredOpeningArea_M2", RequiredOpeningArea_M2);
            PartFJson.SetNullableDouble(result, "ExternalApertureArea_M2", ExternalApertureArea_M2);
            PartFJson.SetNullableDouble(result, "OpeningAngle_Degrees", OpeningAngle_Degrees);
            PartFJson.SetNullableDouble(result, "OpenableWindowArea_M2", OpenableWindowArea_M2);
            PartFJson.SetNullableDouble(result, "ExternalDoorOpeningArea_M2", ExternalDoorOpeningArea_M2);
            PartFJson.SetNullableDouble(result, "MechanicalPurgeCapacity_Lps", MechanicalPurgeCapacity_Lps);

            result["PurgeMethod"] = PurgeMethod.ToString();
            result["OpeningType"] = OpeningType.ToString();
            result["ComplianceStatus"] = ComplianceStatus.ToString();

            PartFJson.SetString(result, "SourceReference", SourceReference);
            PartFJson.SetString(result, "Diagnostic", Diagnostic);
            PartFJson.SetString(result, "PartOInteractionNote", PartOInteractionNote);

            return result;
        }
    }
}
