// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One ventilation terminal required in one space by Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England), with the rates that apply to it at each operating condition.
    /// <para>
    /// The terminal, not the space, is the unit of the Part F calculation. A space can require more than
    /// one: Appendix A (page 36) makes a studio and an open plan living kitchen habitable rooms, because
    /// neither is <i>solely</i> a kitchen, so paragraph 1.67 (page 16) requires mechanical supply to
    /// them; and both contain the cooking function, so paragraph 1.17a (page 8) and Table 1.2 (page 10)
    /// require kitchen extract from them as well. Representing only one role per space cannot express
    /// that, and previously left the local kitchen extract unmodelled.
    /// </para>
    /// <para>
    /// Each rate is held at its own operating condition and the conditions are never combined:
    /// <see cref="ContinuousDesignFlowRate_Lps"/> is the Approved Document F sizing case,
    /// <see cref="HighFlowRate_Lps"/> is the Table 1.2 high rate condition,
    /// <see cref="SetbackFlowRate_Lps"/> is a SAM reduced-operation convention, and the measured rates
    /// are commissioning evidence that never overwrites a design value.
    /// </para>
    /// </summary>
    public class PartFVentilationTerminalRequirement : SAMObject
    {
        public PartFVentilationTerminalRequirement()
        {
        }

        public PartFVentilationTerminalRequirement(string name, Guid spaceGuid, PartFTerminalRole terminalRole)
            : base(name)
        {
            SpaceGuid = spaceGuid;
            TerminalRole = terminalRole;
        }

        public PartFVentilationTerminalRequirement(PartFVentilationTerminalRequirement partFVentilationTerminalRequirement)
            : base(partFVentilationTerminalRequirement)
        {
            if (partFVentilationTerminalRequirement is not null)
            {
                SpaceGuid = partFVentilationTerminalRequirement.SpaceGuid;
                SpaceName = partFVentilationTerminalRequirement.SpaceName;
                TerminalRole = partFVentilationTerminalRequirement.TerminalRole;
                ExtractMethod = partFVentilationTerminalRequirement.ExtractMethod;
                ProposedExtractMethod = partFVentilationTerminalRequirement.ProposedExtractMethod;
                ProvidedExtractMethod = partFVentilationTerminalRequirement.ProvidedExtractMethod;
                ProvisionStatus = partFVentilationTerminalRequirement.ProvisionStatus;
                OperatingMode = partFVentilationTerminalRequirement.OperatingMode;
                ContinuousDesignFlowRate_Lps = partFVentilationTerminalRequirement.ContinuousDesignFlowRate_Lps;
                HighFlowRate_Lps = partFVentilationTerminalRequirement.HighFlowRate_Lps;
                SetbackFlowRate_Lps = partFVentilationTerminalRequirement.SetbackFlowRate_Lps;
                MinimumRequiredFlowRate_Lps = partFVentilationTerminalRequirement.MinimumRequiredFlowRate_Lps;
                MeasuredContinuousFlowRate_Lps = partFVentilationTerminalRequirement.MeasuredContinuousFlowRate_Lps;
                MeasuredHighFlowRate_Lps = partFVentilationTerminalRequirement.MeasuredHighFlowRate_Lps;
                IsLocalExtract = partFVentilationTerminalRequirement.IsLocalExtract;
                IsRequired = partFVentilationTerminalRequirement.IsRequired;
                IsInBalancedFlow = partFVentilationTerminalRequirement.IsInBalancedFlow;
                HighRateIncreaseRequired = partFVentilationTerminalRequirement.HighRateIncreaseRequired;
                SourceReference = partFVentilationTerminalRequirement.SourceReference;
                ComplianceStatus = partFVentilationTerminalRequirement.ComplianceStatus;
                Diagnostic = partFVentilationTerminalRequirement.Diagnostic;
            }
        }

        public PartFVentilationTerminalRequirement(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>Identifier of the space this terminal serves.</summary>
        public Guid SpaceGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Name of the space this terminal serves, held so a terminal schedule reads without having to
        /// resolve every guid back through the model.
        /// </summary>
        public string SpaceName { get; set; }

        /// <summary>What this terminal does. See <see cref="PartFTerminalRole"/>.</summary>
        public PartFTerminalRole TerminalRole { get; set; } = PartFTerminalRole.Undefined;

        /// <summary>
        /// The extract method the sizing worked to: <see cref="ProvidedExtractMethod"/> where one is
        /// recorded, and <see cref="ProposedExtractMethod"/> otherwise. It decides both the applicable
        /// rate table and whether the terminal is part of the balanced mechanical ventilation with heat
        /// recovery flow. Always <see cref="PartFExtractMethod.NotRepresented"/> for a supply terminal.
        /// <para>
        /// A rate has to be sized from something, so this is never
        /// <see cref="PartFExtractMethod.NotSpecified"/> on an extract terminal. Read
        /// <see cref="ProvisionStatus"/>, not this, to find out whether the arrangement is actually
        /// recorded: an assumed method sizes a terminal but proves nothing about the built work.
        /// </para>
        /// </summary>
        public PartFExtractMethod ExtractMethod { get; set; } = PartFExtractMethod.NotRepresented;

        /// <summary>
        /// The extract method SAM proposes for this location, from the system type and the room. For a
        /// cooking space on a balanced system that is a continuous mechanical ventilation with heat
        /// recovery terminal local to the cooking area.
        /// <para>
        /// A proposal, not an observation. It exists so the assessment can size and schedule a terminal
        /// the design needs, and it is never evidence that the terminal is there.
        /// </para>
        /// </summary>
        public PartFExtractMethod ProposedExtractMethod { get; set; } = PartFExtractMethod.NotSpecified;

        /// <summary>
        /// The extract method actually recorded for this location, by the model or by a person.
        /// <see cref="PartFExtractMethod.NotSpecified"/> until one is.
        /// <para>
        /// This is the distinction that keeps a proposed kitchen terminal out of the compliance result as
        /// though it were installed. Paragraph 1.17 requires extract ventilation <b>to the outside</b>,
        /// and a recirculating cooker hood is expressly not an acceptable external extract provision
        /// (Diagram 1.2 note 1, page 9) - so what has actually been provided is a fact about the design,
        /// not something a calculation may fill in for itself.
        /// </para>
        /// </summary>
        public PartFExtractMethod ProvidedExtractMethod { get; set; } = PartFExtractMethod.NotSpecified;

        /// <summary>
        /// Whether the provision this terminal requires has actually been established.
        /// <para>
        /// Separate from <see cref="ComplianceStatus"/>, which is the outcome of assessing the terminal
        /// against its rate requirement. A terminal can be sized correctly to the last decimal and still
        /// have <see cref="PartFComplianceStatus.CannotBeDetermined"/> here, because nobody has said what
        /// is installed.
        /// </para>
        /// </summary>
        public PartFComplianceStatus ProvisionStatus { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>
        /// True where <see cref="ProvidedExtractMethod"/> records an actual arrangement, so the terminal
        /// rests on something the model or a person stated rather than on
        /// <see cref="ProposedExtractMethod"/>.
        /// </summary>
        public bool IsProvisionRecorded
        {
            get { return ProvidedExtractMethod != PartFExtractMethod.NotSpecified; }
        }

        /// <summary>
        /// The condition the terminal normally runs at. Continuous for a mechanical ventilation with
        /// heat recovery terminal (Appendix A, continuous operation); high/boost for a terminal that only
        /// runs intermittently, such as a cooker hood.
        /// </summary>
        public PartFOperatingMode OperatingMode { get; set; } = PartFOperatingMode.ContinuousDesign;

        /// <summary>
        /// Design flow rate [l/s] at the continuous design condition - the Approved Document F sizing
        /// case. Null where the terminal does not run continuously, e.g. a cooker hood or a separate
        /// intermittent extract fan.
        /// </summary>
        public double? ContinuousDesignFlowRate_Lps { get; set; }

        /// <summary>
        /// Design flow rate [l/s] at the high/boost condition. For an extract terminal on the mechanical
        /// ventilation with heat recovery system this is at least the Table 1.2 minimum high rate; for an
        /// intermittent arrangement it is the Table 1.1 rate. For a supply terminal it is the balancing
        /// share of the total high extract.
        /// </summary>
        public double? HighFlowRate_Lps { get; set; }

        /// <summary>
        /// Flow rate [l/s] at the SAM setback operating condition, i.e. the continuous design rate scaled
        /// by the rule set's setback factor. Not a regulatory rate and never checked against any minimum.
        /// </summary>
        public double? SetbackFlowRate_Lps { get; set; }

        /// <summary>
        /// The regulatory minimum [l/s] this terminal is checked against, from Table 1.2 (continuous
        /// system high rate) or Table 1.1 (intermittent system rate) depending on
        /// <see cref="ExtractMethod"/>. Null for a supply terminal, which carries no per room minimum -
        /// paragraph 1.67 sets only the distribution rule, not a floor for each room.
        /// </summary>
        public double? MinimumRequiredFlowRate_Lps { get; set; }

        /// <summary>
        /// The high rate [l/s] the Approved Document requires of this terminal.
        /// <para>Retained as an alias of <see cref="MinimumRequiredFlowRate_Lps"/>, which has always held
        /// exactly that: the Table 1.2 figures are minimum <i>high</i> rates. Named this way so the
        /// requirement, the proposal and the provision read as the three separate things they are.</para>
        /// </summary>
        public double? RequiredHighFlowRate_Lps
        {
            get { return MinimumRequiredFlowRate_Lps; }
            set { MinimumRequiredFlowRate_Lps = value; }
        }

        /// <summary>
        /// Measured continuous air flow rate [l/s] recorded at commissioning, per Section 4 paragraph
        /// 4.10 and Appendix C Part 3. Held separately from the design rate and never written over it.
        /// </summary>
        public double? MeasuredContinuousFlowRate_Lps { get; set; }

        /// <summary>
        /// Measured high air flow rate [l/s] recorded at commissioning, per Section 4 paragraph 4.10 and
        /// Appendix C Part 3.
        /// </summary>
        public double? MeasuredHighFlowRate_Lps { get; set; }

        /// <summary>
        /// True where this terminal is local to the pollutant source it serves, which for Approved
        /// Document F means the cooking function. Extract from a bathroom, ensuite or utility room is
        /// general extract and is not local kitchen extract, however much air it moves.
        /// </summary>
        public bool IsLocalExtract { get; set; }

        /// <summary>
        /// True where the Approved Document requires this terminal to exist. A terminal can be required
        /// and still be absent from the design, which is exactly the case the assessment must report.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// True where this terminal's flow forms part of the balanced continuous mechanical ventilation
        /// with heat recovery design flow, so that total supply equals total extract.
        /// <para>
        /// False for a cooker hood, a separate intermittent extract fan and a recirculating hood.
        /// Paragraph 1.18 allows extract to be intermittent, and an intermittent device does not run at
        /// the continuous design condition, so counting it would credit the balanced system with air it
        /// does not move.
        /// </para>
        /// </summary>
        public bool IsInBalancedFlow { get; set; }

        /// <summary>
        /// True where the continuous rate is below the Table 1.2 minimum high rate, so the terminal has
        /// to be able to boost. Table 1.2 note 1 (page 10): "If the continuous rate of ventilation
        /// provided in a room is equal to or higher than the minimum high rate specified in the table, no
        /// extra ventilation is needed."
        /// </summary>
        public bool HighRateIncreaseRequired { get; set; }

        /// <summary>The Approved Document paragraph or table this terminal's requirement comes from.</summary>
        public string SourceReference { get; set; }

        /// <summary>Outcome of assessing this terminal against its requirement.</summary>
        public PartFComplianceStatus ComplianceStatus { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>Why the terminal reached that status, in the engineer's language.</summary>
        public string Diagnostic { get; set; }

        /// <summary>
        /// True where the terminal moves air out of the space at the continuous design condition, so it
        /// counts negatively towards the space's net airflow.
        /// </summary>
        public bool IsExtract
        {
            get
            {
                return TerminalRole == PartFTerminalRole.GeneralExtract || TerminalRole == PartFTerminalRole.LocalKitchenExtract;
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            Guid spaceGuid = PartFJson.Guid(jsonObject, "SpaceGuid");
            if (spaceGuid != Guid.Empty)
            {
                SpaceGuid = spaceGuid;
            }

            SpaceName = PartFJson.String(jsonObject, "SpaceName") ?? SpaceName;

            if (jsonObject.ContainsKey("TerminalRole"))
            {
                TerminalRole = Core.Query.Enum<PartFTerminalRole>(PartFJson.String(jsonObject, "TerminalRole"));
            }

            if (jsonObject.ContainsKey("ExtractMethod"))
            {
                ExtractMethod = Core.Query.Enum<PartFExtractMethod>(PartFJson.String(jsonObject, "ExtractMethod"));
            }

            if (jsonObject.ContainsKey("ProposedExtractMethod"))
            {
                ProposedExtractMethod = Core.Query.Enum<PartFExtractMethod>(PartFJson.String(jsonObject, "ProposedExtractMethod"));
            }

            if (jsonObject.ContainsKey("ProvidedExtractMethod"))
            {
                ProvidedExtractMethod = Core.Query.Enum<PartFExtractMethod>(PartFJson.String(jsonObject, "ProvidedExtractMethod"));
            }

            if (jsonObject.ContainsKey("ProvisionStatus"))
            {
                ProvisionStatus = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "ProvisionStatus"));
            }

            if (jsonObject.ContainsKey("OperatingMode"))
            {
                OperatingMode = Core.Query.Enum<PartFOperatingMode>(PartFJson.String(jsonObject, "OperatingMode"));
            }

            ContinuousDesignFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "ContinuousDesignFlowRate_Lps");
            HighFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "HighFlowRate_Lps");
            SetbackFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "SetbackFlowRate_Lps");
            MinimumRequiredFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "MinimumRequiredFlowRate_Lps");
            MeasuredContinuousFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredContinuousFlowRate_Lps");
            MeasuredHighFlowRate_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredHighFlowRate_Lps");

            IsLocalExtract = PartFJson.Boolean(jsonObject, "IsLocalExtract", IsLocalExtract);
            IsRequired = PartFJson.Boolean(jsonObject, "IsRequired", IsRequired);
            IsInBalancedFlow = PartFJson.Boolean(jsonObject, "IsInBalancedFlow", IsInBalancedFlow);
            HighRateIncreaseRequired = PartFJson.Boolean(jsonObject, "HighRateIncreaseRequired", HighRateIncreaseRequired);

            SourceReference = PartFJson.String(jsonObject, "SourceReference") ?? SourceReference;

            if (jsonObject.ContainsKey("ComplianceStatus"))
            {
                ComplianceStatus = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "ComplianceStatus"));
            }

            Diagnostic = PartFJson.String(jsonObject, "Diagnostic") ?? Diagnostic;

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

            result["TerminalRole"] = TerminalRole.ToString();
            result["ExtractMethod"] = ExtractMethod.ToString();
            result["ProposedExtractMethod"] = ProposedExtractMethod.ToString();
            result["ProvidedExtractMethod"] = ProvidedExtractMethod.ToString();
            result["ProvisionStatus"] = ProvisionStatus.ToString();
            result["OperatingMode"] = OperatingMode.ToString();

            PartFJson.SetNullableDouble(result, "ContinuousDesignFlowRate_Lps", ContinuousDesignFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "HighFlowRate_Lps", HighFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "SetbackFlowRate_Lps", SetbackFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "MinimumRequiredFlowRate_Lps", MinimumRequiredFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "MeasuredContinuousFlowRate_Lps", MeasuredContinuousFlowRate_Lps);
            PartFJson.SetNullableDouble(result, "MeasuredHighFlowRate_Lps", MeasuredHighFlowRate_Lps);

            result["IsLocalExtract"] = IsLocalExtract;
            result["IsRequired"] = IsRequired;
            result["IsInBalancedFlow"] = IsInBalancedFlow;
            result["HighRateIncreaseRequired"] = HighRateIncreaseRequired;

            PartFJson.SetString(result, "SourceReference", SourceReference);

            result["ComplianceStatus"] = ComplianceStatus.ToString();

            PartFJson.SetString(result, "Diagnostic", Diagnostic);

            return result;
        }
    }
}
