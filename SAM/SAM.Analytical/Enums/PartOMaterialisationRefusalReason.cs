// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Why a model is not a clean Part O baseline, or why a set of dwelling strategies could not be
    /// materialised. The structured half of a <c>PartOMaterialisationRefusal</c>; its message is the other half.
    /// <para>Members are appended, never reordered.</para>
    /// </summary>
    [Description("Part O Materialisation Refusal Reason.")]
    public enum PartOMaterialisationRefusalReason
    {
        [Description("Undefined")] Undefined,

        // ---- the model supplied -----------------------------------------------------------------------

        /// <summary>No model, or no adjacency cluster.</summary>
        [Description("No Model")] NoModel,

        /// <summary>The model already carries Part O materialisation state (D1).</summary>
        [Description("Materialised Baseline")] MaterialisedBaseline,

        /// <summary>The model carries run output: scenarios, provenance, results or run-written design days (D1).</summary>
        [Description("Run Output Baseline")] RunOutputBaseline,

        // ---- the strategy set -------------------------------------------------------------------------

        /// <summary>The model carries no dwelling strategy collection: a legacy model - not an error, but no mixed authority either.</summary>
        [Description("No Strategies")] NoStrategies,

        /// <summary>The strategy collection is unreadable or of an unknown schema.</summary>
        [Description("Invalid Strategy Set")] InvalidStrategySet,

        /// <summary>A strategy states an undefined or contradictory property.</summary>
        [Description("Invalid Strategy")] InvalidStrategy,

        /// <summary>A strategy or the assessed scope names a zone the model does not contain.</summary>
        [Description("Unknown Zone")] UnknownZone,

        /// <summary>A strategy or the assessed scope names a zone that is not a dwelling.</summary>
        [Description("Not A Dwelling")] NotADwelling,

        /// <summary>An assessed dwelling has no selected strategy. Absence is never read as natural ventilation.</summary>
        [Description("Missing Strategy")] MissingStrategy,

        /// <summary>A space belongs to more than one assessed zone, so it has no single strategy.</summary>
        [Description("Overlapping Zones")] OverlappingZones,

        // ---- contradictions and gates -----------------------------------------------------------------

        /// <summary>
        /// Retired with PR3 - active cooling is materialised. Kept so persisted values keep their meaning; nothing
        /// produces it.
        /// </summary>
        [Description("Cooling Gated")] CoolingGated,

        /// <summary>Natural ventilation with active supply-air cooling: the only cooling path is the MVHR supply.</summary>
        [Description("Natural With Cooling")] NaturalWithCooling,

        /// <summary>Natural ventilation with a retained mechanical design.</summary>
        [Description("Natural With Retained Design")] NaturalWithRetainedDesign,

        /// <summary>Natural ventilation with a selected ventilation unit product.</summary>
        [Description("Natural With Ventilation Unit")] NaturalWithVentilationUnit,

        /// <summary>A naturally ventilated dwelling is served by authored mechanical duty or plant.</summary>
        [Description("Natural Over Mechanical Duty")] NaturalOverMechanicalDuty,

        /// <summary>An authored mechanical system or unit straddles zones, so it cannot belong to one dwelling's design.</summary>
        [Description("Shared System")] SharedSystem,

        /// <summary>An MVHR dwelling carries authored plant that is not connected to its design terminals.</summary>
        [Description("Unconnected Authored Plant")] UnconnectedAuthoredPlant,

        /// <summary>A reused authored unit states a supply temperature: conditioning behind the cooling gate (P12).</summary>
        [Description("Conditioned Reused Unit")] ConditionedReusedUnit,

        // ---- design airflow ---------------------------------------------------------------------------

        /// <summary>A retained design no longer matches its fingerprint, or there is no design to retain.</summary>
        [Description("Retained Design Stale")] RetainedDesignStale,

        /// <summary>A Part F requirement basis over design terminals that differ from the requirement.</summary>
        [Description("Design Differs From Requirement")] DesignDiffersFromRequirement,

        // ---- equipment --------------------------------------------------------------------------------

        /// <summary>A selected ventilation unit product is not in the catalogue offered, or no catalogue was offered.</summary>
        [Description("Ventilation Unit Unresolved")] VentilationUnitUnresolved,

        /// <summary>A selected ventilation unit product is not permitted by the project's equipment preselection.</summary>
        [Description("Ventilation Unit Not Allowed")] VentilationUnitNotAllowed,

        /// <summary>No product could be selected for the dwelling's duty, or the selected one cannot serve it.</summary>
        [Description("Ventilation Unit Selection")] VentilationUnitSelection,

        // ---- the realisation itself -------------------------------------------------------------------

        /// <summary>The Approved Document F rates or terminals could not be applied.</summary>
        [Description("Part F Application")] PartFApplication,

        /// <summary>The dwelling's MVHR design could not be realised (system, movements, transfer air, balance).</summary>
        [Description("Mechanical Design")] MechanicalDesign,

        /// <summary>A common-space zone mixes communal-corridor spaces and other spaces, so it has no single criterion.</summary>
        [Description("Common Space Unclassifiable")] CommonSpaceUnclassifiable,

        /// <summary>A scenario could not be stated.</summary>
        [Description("Scenario")] Scenario,

        /// <summary>A post-materialisation invariant failed.</summary>
        [Description("Invariant")] Invariant,

        /// <summary>
        /// An air movement states an endpoint the model does not contain, so TAS would drop it or send the air
        /// to outside instead of where the model says.
        /// </summary>
        [Description("Unresolved Air Movement")] UnresolvedAirMovement,

        /// <summary>
        /// An authored air movement reaches an MVHR dwelling (or the unit serving one), whose runtime air network
        /// the materialisation rebuilds: it would be deleted, or the dwelling ventilated twice.
        /// </summary>
        [Description("Authored Air Movement Conflict")] AuthoredAirMovementConflict,

        /// <summary>
        /// A cooled dwelling has no product whose catalogue entry states the manufacturer's cooling operating
        /// strategy - a generic unit, the project test unit, or a product without guidance. Cooling is the product's
        /// own; nothing else is its authority.
        /// </summary>
        [Description("Cooling Without Product Guidance")] CoolingWithoutProductGuidance,

        /// <summary>
        /// A cooled dwelling's cooling operating airflow falls outside the manufacturer's published cooling airflow
        /// range or the selected unit's capacity.
        /// </summary>
        [Description("Cooling Airflow Outside Guidance")] CoolingAirFlowOutsideGuidance,

        /// <summary>The cooling stat room has not been confirmed or is not a space of its dwelling.</summary>
        [Description("Cooling Control Room Selection")] CoolingControlRoomSelection,
    }
}
