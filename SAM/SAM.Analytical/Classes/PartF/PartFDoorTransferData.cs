// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The Approved Document F transfer air requirement for one internal route between two spaces of a
    /// single dwelling, and what the model can say about whether that route provides it.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England) paragraph 1.25
    /// (page 10): "Internal doors should allow air to flow through the dwelling by providing a minimum
    /// free area equivalent to a 10mm undercut in a 760mm wide door. Doors should be undercut to achieve
    /// one of the following. a. If the floor finish is fitted: 10mm above the floor finish. b. If the
    /// floor finish is not fitted: 20mm above the floor surface."
    /// </para>
    /// <para>
    /// The equivalent free area is therefore 10mm x 760mm = 7,600mm2. That product is arithmetic, not a
    /// number printed in the Approved Document, which states the equivalence rather than the area; it is
    /// held as <see cref="NominalEquivalentFreeArea_mm2"/> so the derivation is visible.
    /// </para>
    /// <para>
    /// <b>The Part F requirement on an internal door is an AREA, never a flow rate.</b> Paragraph 1.25
    /// asks for a free area equivalent to a 10mm undercut in a 760mm wide door and does not prescribe an
    /// air flow through any individual door. The l/s values on this record - see
    /// <see cref="ContinuousDesignTransferFlowRate_Lps"/> and its siblings - are therefore SAM's own
    /// calculated airflow-network routing, produced by conserving air across the dwelling so that the
    /// schematic and the space airflow balance add up. They are engineering information about where the
    /// air goes; they are not a Part F door-flow requirement, nothing is checked against them, and a door
    /// neither passes nor fails on their value.
    /// </para>
    /// <para>
    /// Two kinds of value live on this record and they behave differently across a recalculation. The
    /// <b>requirement</b> and the <b>calculated transfer flow</b> are derived and are rewritten every
    /// time the dwelling is sized. The <b>provided</b> values, the transfer device type, the floor finish
    /// state and any transfer flow override are engineering inputs: SAM cannot see an undercut in the
    /// analytical model, so they are carried forward from the previous record and never overwritten by
    /// the calculation.
    /// </para>
    /// </summary>
    public class PartFDoorTransferData : SAMObject
    {
        /// <summary>
        /// Width [mm] of the reference door in paragraph 1.25 (page 10).
        /// </summary>
        public const double ReferenceDoorWidth_mm = 760;

        /// <summary>
        /// Undercut height [mm] of the reference door in paragraph 1.25 (page 10), and the required
        /// undercut where the floor finish is fitted (paragraph 1.25a).
        /// </summary>
        public const double ReferenceUndercutHeight_mm = 10;

        /// <summary>
        /// Required undercut height [mm] measured above the floor surface where the floor finish is NOT
        /// fitted (paragraph 1.25b, page 10).
        /// </summary>
        public const double UndercutHeightBeforeFloorFinish_mm = 20;

        /// <summary>
        /// Minimum free area [mm2] equivalent to a 10mm undercut in a 760mm wide door, i.e.
        /// <see cref="ReferenceUndercutHeight_mm"/> x <see cref="ReferenceDoorWidth_mm"/> = 7,600mm2.
        /// </summary>
        public const double NominalEquivalentFreeArea_mm2 = ReferenceUndercutHeight_mm * ReferenceDoorWidth_mm;

        public PartFDoorTransferData()
        {
        }

        public PartFDoorTransferData(string name)
            : base(name)
        {
        }

        public PartFDoorTransferData(PartFDoorTransferData partFDoorTransferData)
            : base(partFDoorTransferData)
        {
            if (partFDoorTransferData is not null)
            {
                ApertureGuid = partFDoorTransferData.ApertureGuid;
                UpstreamSpaceGuid = partFDoorTransferData.UpstreamSpaceGuid;
                DownstreamSpaceGuid = partFDoorTransferData.DownstreamSpaceGuid;
                UpstreamSpaceName = partFDoorTransferData.UpstreamSpaceName;
                DownstreamSpaceName = partFDoorTransferData.DownstreamSpaceName;
                DwellingName = partFDoorTransferData.DwellingName;
                RequiresTransferAirPath = partFDoorTransferData.RequiresTransferAirPath;
                IsInternalDwellingDoor = partFDoorTransferData.IsInternalDwellingDoor;
                IsDoorRepresented = partFDoorTransferData.IsDoorRepresented;
                ContinuousDesignTransferFlowRate_Lps = partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps;
                HighTransferFlowRate_Lps = partFDoorTransferData.HighTransferFlowRate_Lps;
                SetbackTransferFlowRate_Lps = partFDoorTransferData.SetbackTransferFlowRate_Lps;
                MinimumRequiredFreeArea_mm2 = partFDoorTransferData.MinimumRequiredFreeArea_mm2;
                RequiredUndercutHeightFinished_mm = partFDoorTransferData.RequiredUndercutHeightFinished_mm;
                RequiredUndercutHeightBeforeFloorFinish_mm = partFDoorTransferData.RequiredUndercutHeightBeforeFloorFinish_mm;
                ProvidedUndercutHeight_mm = partFDoorTransferData.ProvidedUndercutHeight_mm;
                ProvidedFreeArea_mm2 = partFDoorTransferData.ProvidedFreeArea_mm2;
                ClearDoorWidth_mm = partFDoorTransferData.ClearDoorWidth_mm;
                IsFloorFinishFitted = partFDoorTransferData.IsFloorFinishFitted;
                TransferDeviceType = partFDoorTransferData.TransferDeviceType;
                TransferFlowRateOverride_Lps = partFDoorTransferData.TransferFlowRateOverride_Lps;
                RouteStatus = partFDoorTransferData.RouteStatus;
                ComplianceStatus = partFDoorTransferData.ComplianceStatus;
                SourceReference = partFDoorTransferData.SourceReference;
                CalculationSource = partFDoorTransferData.CalculationSource;
                Diagnostic = partFDoorTransferData.Diagnostic;
            }
        }

        public PartFDoorTransferData(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        // ------------------------------------------------------------------
        // Identity
        // ------------------------------------------------------------------

        /// <summary>
        /// The door aperture this record belongs to, or <see cref="Guid.Empty"/> where the two spaces are
        /// adjacent through an internal partition that carries no modelled door.
        /// </summary>
        public Guid ApertureGuid { get; set; } = Guid.Empty;

        /// <summary>The space air flows FROM at the continuous design condition.</summary>
        public Guid UpstreamSpaceGuid { get; set; } = Guid.Empty;

        /// <summary>The space air flows TO at the continuous design condition.</summary>
        public Guid DownstreamSpaceGuid { get; set; } = Guid.Empty;

        /// <summary>Name of the upstream space, held so a schedule reads without resolving guids.</summary>
        public string UpstreamSpaceName { get; set; }

        /// <summary>Name of the downstream space, held so a schedule reads without resolving guids.</summary>
        public string DownstreamSpaceName { get; set; }

        /// <summary>Name of the dwelling this route belongs to, or null in single dwelling mode.</summary>
        public string DwellingName { get; set; }

        // ------------------------------------------------------------------
        // Requirement (derived - recalculated every run)
        // ------------------------------------------------------------------

        /// <summary>
        /// True where paragraph 1.25 requires this route to allow air to flow through the dwelling.
        /// </summary>
        public bool RequiresTransferAirPath { get; set; }

        /// <summary>
        /// True where both spaces belong to the same dwelling. An external door, a dwelling entrance door
        /// onto a communal area, and a door between two dwellings are all false, and carry no Part F
        /// internal transfer requirement.
        /// </summary>
        public bool IsInternalDwellingDoor { get; set; }

        /// <summary>
        /// True where a door aperture is actually modelled on the separating element. False means the two
        /// spaces are adjacent and air has to move between them, but the opening itself is not in the
        /// model, so nothing about its undercut can be checked.
        /// </summary>
        public bool IsDoorRepresented { get; set; }

        /// <summary>
        /// Minimum free area [mm2] required by paragraph 1.25, i.e.
        /// <see cref="NominalEquivalentFreeArea_mm2"/>.
        /// </summary>
        public double? MinimumRequiredFreeArea_mm2 { get; set; }

        /// <summary>
        /// Undercut [mm] required above the floor finish where the finish is fitted (paragraph 1.25a).
        /// </summary>
        public double? RequiredUndercutHeightFinished_mm { get; set; }

        /// <summary>
        /// Undercut [mm] required above the floor surface where the finish is not fitted
        /// (paragraph 1.25b).
        /// </summary>
        public double? RequiredUndercutHeightBeforeFloorFinish_mm { get; set; }

        // ------------------------------------------------------------------
        // Calculated airflow-network routing (derived - recalculated every run)
        //
        // NOT a Part F requirement. Paragraph 1.25 requires a free AREA through an internal door and
        // prescribes no flow rate for one. These values come from conserving air across the dwelling.
        // ------------------------------------------------------------------

        /// <summary>
        /// Calculated transfer air flow [l/s] routed through this opening at the continuous design
        /// condition, positive from <see cref="UpstreamSpaceName"/> to
        /// <see cref="DownstreamSpaceName"/>.
        /// <para>
        /// SAM's airflow-network result, not an Approved Document F requirement. Paragraph 1.25 requires
        /// a free area through an internal door and prescribes no flow rate for one, so nothing is
        /// assessed against this number.
        /// </para>
        /// </summary>
        public double? ContinuousDesignTransferFlowRate_Lps { get; set; }

        /// <summary>
        /// Calculated transfer air flow [l/s] routed through this opening with the system at its high
        /// rate. Airflow-network routing, not a Part F requirement.
        /// </summary>
        public double? HighTransferFlowRate_Lps { get; set; }

        /// <summary>
        /// Calculated transfer air flow [l/s] routed through this opening at the SAM setback operating
        /// condition. Airflow-network routing, not a Part F requirement.
        /// </summary>
        public double? SetbackTransferFlowRate_Lps { get; set; }

        /// <summary>How the transfer flow was arrived at. See <see cref="PartFTransferRouteStatus"/>.</summary>
        public PartFTransferRouteStatus RouteStatus { get; set; } = PartFTransferRouteStatus.NotAssessed;

        /// <summary>Plain description of the calculation that produced the transfer flow.</summary>
        public string CalculationSource { get; set; }

        // ------------------------------------------------------------------
        // Engineering input (carried forward - never overwritten by the calculation)
        // ------------------------------------------------------------------

        /// <summary>
        /// Undercut [mm] actually provided. An engineering input: the analytical model does not represent
        /// a gap under a door leaf, and a door's graphical height is not evidence of one.
        /// </summary>
        public double? ProvidedUndercutHeight_mm { get; set; }

        /// <summary>
        /// Free area [mm2] actually provided, whether by an undercut, a transfer grille or a permanent
        /// opening. An engineering input.
        /// </summary>
        public double? ProvidedFreeArea_mm2 { get; set; }

        /// <summary>
        /// Clear width [mm] of the door leaf, used with <see cref="ProvidedUndercutHeight_mm"/> to derive
        /// the provided free area where the area itself was not entered. Taken from the door aperture
        /// geometry where one is modelled, and otherwise an engineering input.
        /// </summary>
        public double? ClearDoorWidth_mm { get; set; }

        /// <summary>
        /// True where the floor finish is fitted, which selects the 10mm requirement of paragraph 1.25a
        /// rather than the 20mm requirement of paragraph 1.25b. Null where it is not known, in which case
        /// both requirements are reported and neither is assumed.
        /// </summary>
        public bool? IsFloorFinishFitted { get; set; }

        /// <summary>How air is allowed to move on this route. See <see cref="PartFTransferDeviceType"/>.</summary>
        public PartFTransferDeviceType TransferDeviceType { get; set; } = PartFTransferDeviceType.NotRepresented;

        /// <summary>
        /// Transfer air flow [l/s] entered by the engineer, which replaces the calculated routing for
        /// this opening. Provided because Approved Document F specifies no flow through an individual
        /// door at all, and where several airflow paths exist the split between them is a design decision
        /// rather than a regulatory value.
        /// </summary>
        public double? TransferFlowRateOverride_Lps { get; set; }

        // ------------------------------------------------------------------
        // Assessment
        // ------------------------------------------------------------------

        /// <summary>Outcome of assessing the provided transfer area against paragraph 1.25.</summary>
        public PartFComplianceStatus ComplianceStatus { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>
        /// True only where the requirement was positively shown to be met. Absent evidence is not
        /// compliance, so <see cref="PartFComplianceStatus.CannotBeDetermined"/> is false here.
        /// </summary>
        public bool IsCompliant
        {
            get
            {
                return ComplianceStatus == PartFComplianceStatus.Pass || ComplianceStatus == PartFComplianceStatus.UserConfirmed;
            }
        }

        /// <summary>
        /// What the model shows about the physical opening on this route, worst case first. See
        /// <see cref="PartFTransferOpeningStatus"/>.
        /// <para>
        /// Derived on every read from the evidence already on this record, so it can never drift out of
        /// step with it. Any surface that draws or summarises a route should style it from this rather
        /// than from the flow: a calculated litres-per-second figure says nothing about whether the air
        /// has anywhere to go.
        /// </para>
        /// </summary>
        public PartFTransferOpeningStatus OpeningStatus
        {
            get
            {
                if (RouteStatus == PartFTransferRouteStatus.NotAssessed && ComplianceStatus == PartFComplianceStatus.NotAssessed)
                {
                    return PartFTransferOpeningStatus.NotAssessed;
                }

                bool hasDevice = TransferDeviceType != PartFTransferDeviceType.NotRepresented;

                //Nothing found at all comes first, ahead of every question about the flow. A route with no
                //opening is not a route whose split needs deciding; it is a route the model does not show.
                if (!IsDoorRepresented && !hasDevice && EffectiveProvidedFreeArea_mm2() is null)
                {
                    return PartFTransferOpeningStatus.MissingTransferOpening;
                }

                if (RouteStatus == PartFTransferRouteStatus.Ambiguous
                    || RouteStatus == PartFTransferRouteStatus.AllocationStrategy
                    || RouteStatus == PartFTransferRouteStatus.NotCalculable)
                {
                    return PartFTransferOpeningStatus.AmbiguousRoute;
                }

                //A recorded free area that passes paragraph 1.25 is the only evidence here that came from
                //a person rather than from the geometry, and it is the strongest.
                if (IsCompliant)
                {
                    return PartFTransferOpeningStatus.ConfirmedOpening;
                }

                return IsDoorRepresented
                    ? PartFTransferOpeningStatus.CalculatedViaModelledDoor
                    : PartFTransferOpeningStatus.CalculatedViaPermanentOpening;
            }
        }

        /// <summary>
        /// True where the flow on this route was calculated but no physical opening has been established,
        /// so nothing about it may be drawn or reported as confirmed.
        /// </summary>
        public bool IsOpeningUnresolved
        {
            get
            {
                return OpeningStatus == PartFTransferOpeningStatus.MissingTransferOpening
                    || OpeningStatus == PartFTransferOpeningStatus.AmbiguousRoute
                    || OpeningStatus == PartFTransferOpeningStatus.NotAssessed;
            }
        }

        /// <summary>The Approved Document paragraph the requirement comes from.</summary>
        public string SourceReference { get; set; }

        /// <summary>Why the route reached its status, in the engineer's language.</summary>
        public string Diagnostic { get; set; }

        /// <summary>
        /// Copies the engineering inputs from a previous record onto this one, so a recalculation keeps
        /// what only a person could have supplied. Derived values are deliberately not copied.
        /// </summary>
        public void TakeInputsFrom(PartFDoorTransferData partFDoorTransferData)
        {
            if (partFDoorTransferData is null)
            {
                return;
            }

            ProvidedUndercutHeight_mm = partFDoorTransferData.ProvidedUndercutHeight_mm;
            ProvidedFreeArea_mm2 = partFDoorTransferData.ProvidedFreeArea_mm2;
            IsFloorFinishFitted = partFDoorTransferData.IsFloorFinishFitted;
            TransferFlowRateOverride_Lps = partFDoorTransferData.TransferFlowRateOverride_Lps;

            //A device type of NotRepresented is the default rather than a decision, so it does not
            //overwrite a type this run derived from the model.
            if (partFDoorTransferData.TransferDeviceType != PartFTransferDeviceType.NotRepresented)
            {
                TransferDeviceType = partFDoorTransferData.TransferDeviceType;
            }

            //Only accepted where this run found no width of its own: a width read from the door geometry
            //is better evidence than a stale entry.
            ClearDoorWidth_mm ??= partFDoorTransferData.ClearDoorWidth_mm;
        }

        /// <summary>
        /// The free area [mm2] the route is judged on: the entered area where there is one, otherwise the
        /// area implied by the provided undercut across the clear door width. Null where neither is known.
        /// </summary>
        public double? EffectiveProvidedFreeArea_mm2()
        {
            if (ProvidedFreeArea_mm2 is not null)
            {
                return ProvidedFreeArea_mm2;
            }

            if (ProvidedUndercutHeight_mm is null)
            {
                return null;
            }

            //Where no clear width is known the reference 760mm door is used, so an undercut on its own
            //still produces an assessable area. That substitution is reported in the diagnostic.
            double width = ClearDoorWidth_mm ?? ReferenceDoorWidth_mm;

            return ProvidedUndercutHeight_mm.Value * width;
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            ApertureGuid = PartFJson.Guid(jsonObject, "ApertureGuid");
            UpstreamSpaceGuid = PartFJson.Guid(jsonObject, "UpstreamSpaceGuid");
            DownstreamSpaceGuid = PartFJson.Guid(jsonObject, "DownstreamSpaceGuid");

            UpstreamSpaceName = PartFJson.String(jsonObject, "UpstreamSpaceName");
            DownstreamSpaceName = PartFJson.String(jsonObject, "DownstreamSpaceName");
            DwellingName = PartFJson.String(jsonObject, "DwellingName");

            RequiresTransferAirPath = PartFJson.Boolean(jsonObject, "RequiresTransferAirPath");
            IsInternalDwellingDoor = PartFJson.Boolean(jsonObject, "IsInternalDwellingDoor");
            IsDoorRepresented = PartFJson.Boolean(jsonObject, "IsDoorRepresented");

            ContinuousDesignTransferFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "ContinuousDesignTransferFlowRate_Lps");
            HighTransferFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "HighTransferFlowRate_Lps");
            SetbackTransferFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "SetbackTransferFlowRate_Lps");

            MinimumRequiredFreeArea_mm2 = PartFJson.NullableDouble(jsonObject, "MinimumRequiredFreeArea_mm2");
            RequiredUndercutHeightFinished_mm = PartFJson.NullableDouble(jsonObject, "RequiredUndercutHeightFinished_mm");
            RequiredUndercutHeightBeforeFloorFinish_mm = PartFJson.NullableDouble(jsonObject, "RequiredUndercutHeightBeforeFloorFinish_mm");

            ProvidedUndercutHeight_mm = PartFJson.NullableDouble(jsonObject, "ProvidedUndercutHeight_mm");
            ProvidedFreeArea_mm2 = PartFJson.NullableDouble(jsonObject, "ProvidedFreeArea_mm2");
            ClearDoorWidth_mm = PartFJson.NullableDouble(jsonObject, "ClearDoorWidth_mm");
            TransferFlowRateOverride_Lps = PartFJson.NullableDouble(jsonObject, "TransferFlowRateOverride_Lps");

            //Deliberately tri-state: an absent key means the floor finish state is unknown, which is a
            //different answer from "not fitted" and selects a different paragraph 1.25 requirement.
            if (jsonObject.ContainsKey("IsFloorFinishFitted") && jsonObject["IsFloorFinishFitted"] is not null)
            {
                IsFloorFinishFitted = PartFJson.Boolean(jsonObject, "IsFloorFinishFitted");
            }

            if (jsonObject.ContainsKey("TransferDeviceType"))
            {
                TransferDeviceType = Core.Query.Enum<PartFTransferDeviceType>(PartFJson.String(jsonObject, "TransferDeviceType"));
            }

            if (jsonObject.ContainsKey("RouteStatus"))
            {
                RouteStatus = Core.Query.Enum<PartFTransferRouteStatus>(PartFJson.String(jsonObject, "RouteStatus"));
            }

            if (jsonObject.ContainsKey("ComplianceStatus"))
            {
                ComplianceStatus = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "ComplianceStatus"));
            }

            SourceReference = PartFJson.String(jsonObject, "SourceReference");
            CalculationSource = PartFJson.String(jsonObject, "CalculationSource");
            Diagnostic = PartFJson.String(jsonObject, "Diagnostic");

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            result["ApertureGuid"] = ApertureGuid.ToString();
            result["UpstreamSpaceGuid"] = UpstreamSpaceGuid.ToString();
            result["DownstreamSpaceGuid"] = DownstreamSpaceGuid.ToString();

            PartFJson.SetString(result, "UpstreamSpaceName", UpstreamSpaceName);
            PartFJson.SetString(result, "DownstreamSpaceName", DownstreamSpaceName);
            PartFJson.SetString(result, "DwellingName", DwellingName);

            result["RequiresTransferAirPath"] = RequiresTransferAirPath;
            result["IsInternalDwellingDoor"] = IsInternalDwellingDoor;
            result["IsDoorRepresented"] = IsDoorRepresented;

            PartFJson.SetNullableDouble(result, "ContinuousDesignTransferFlowRate_Lps", ContinuousDesignTransferFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "HighTransferFlowRate_Lps", HighTransferFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "SetbackTransferFlowRate_Lps", SetbackTransferFlowRate_Lps);

            PartFJson.SetNullableDouble(result, "MinimumRequiredFreeArea_mm2", MinimumRequiredFreeArea_mm2);
            PartFJson.SetNullableDouble(result, "RequiredUndercutHeightFinished_mm", RequiredUndercutHeightFinished_mm);
            PartFJson.SetNullableDouble(result, "RequiredUndercutHeightBeforeFloorFinish_mm", RequiredUndercutHeightBeforeFloorFinish_mm);

            PartFJson.SetNullableDouble(result, "ProvidedUndercutHeight_mm", ProvidedUndercutHeight_mm);
            PartFJson.SetNullableDouble(result, "ProvidedFreeArea_mm2", ProvidedFreeArea_mm2);
            PartFJson.SetNullableDouble(result, "ClearDoorWidth_mm", ClearDoorWidth_mm);
            PartFJson.SetNullableDouble(result, "TransferFlowRateOverride_Lps", TransferFlowRateOverride_Lps);

            if (IsFloorFinishFitted is not null)
            {
                result["IsFloorFinishFitted"] = IsFloorFinishFitted.Value;
            }

            result["TransferDeviceType"] = TransferDeviceType.ToString();
            result["RouteStatus"] = RouteStatus.ToString();
            result["ComplianceStatus"] = ComplianceStatus.ToString();

            PartFJson.SetString(result, "SourceReference", SourceReference);
            PartFJson.SetString(result, "CalculationSource", CalculationSource);
            PartFJson.SetString(result, "Diagnostic", Diagnostic);

            return result;
        }
    }
}
