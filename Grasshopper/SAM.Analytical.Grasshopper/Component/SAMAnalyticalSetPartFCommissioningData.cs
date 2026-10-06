// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Records Approved Document F Section 4 and Appendix C commissioning evidence on a dwelling zone.
    /// </summary>
    public class SAMAnalyticalSetPartFCommissioningData : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new("a4d1e7c6-2b90-45f3-9c18-7e0d5b3a86f4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        private const string description =
            "SUMMARY\n" +
            "Records the commissioning evidence required by Approved Document F - Ventilation, Volume 1: Dwellings (2021 edition, for use in England) Section 4 and Appendix C onto a dwelling zone, so that SAMAnalytical.CheckPartFCompliance can assess it.\n" +
            "Design and measured values are held SEPARATELY and a measured value never overwrites a design value. That separation is the whole point of the record: Appendix C paragraph C2 compares the two, and a design rate that had silently become a measured rate would destroy the only evidence the comparison rests on.\n" +
            "\n" +
            "INPUTS\n" +
            "_analyticalModel - AnalyticalModel. Required, one model, containing the dwelling zone.\n" +
            "_zoneName - text. Required. Name of the zone representing the dwelling this evidence belongs to. Commissioning is per dwelling, not per space or per terminal: one commissioning sheet covers one installation address.\n" +
            "commissioningDate_ - text, optional. Date of commissioning (Appendix C section 3.5). Free text, so a project's own date convention survives unchanged.\n" +
            "commissioningEngineer_ - text, optional. Commissioning engineer (Appendix C sections 1.4 and 3.5).\n" +
            "installationEngineer_ - text, optional. Installation engineer (Appendix C sections 1.3 and 2a.2).\n" +
            "measurementEquipment_ - text, optional. Air flow measurement equipment, model and serial number (Appendix C section 3.1).\n" +
            "calibrationDate_ - text, optional. Date of the last UKAS calibration of that equipment (Appendix C section 3.1 and paragraph 4.10c(iii)).\n" +
            "measuredContinuousSupply_ - number, optional, l/s. Measured total continuous supply across the dwelling.\n" +
            "measuredContinuousExtract_ - number, optional, l/s. Measured total continuous extract across the dwelling.\n" +
            "measuredHighSupply_ - number, optional, l/s. Measured total high rate supply.\n" +
            "measuredHighExtract_ - number, optional, l/s. Measured total high rate extract.\n" +
            "commissioningNoticeGiven_ - boolean, optional, default false. Whether the commissioning notice required by paragraph 4.1 has been given to the building control body.\n" +
            "airFlowRateNoticeGiven_ - boolean, optional, default false. Whether the notice of measured air flow rates required by paragraph 4.2 has been given.\n" +
            "operatingAndMaintenanceIssued_ - boolean, optional, default false. Whether the operating and maintenance information of paragraphs 4.13 to 4.17 has been issued to the building owner.\n" +
            "homeUserGuideIssued_ - boolean, optional, default false. Whether the Home User Guide of paragraphs 4.18 and 4.19 has been provided.\n" +
            "confirmedChecks_ - text, list, optional. Names of Part F checks a person has confirmed, one per item. Each is recorded as User Confirmed against the named check.\n" +
            "confirmedBy_ - text, optional. Who confirmed those checks.\n" +
            "confirmedOn_ - text, optional. When they confirmed them.\n" +
            "notes_ - text, optional. Any qualification to carry with the record.\n" +
            "\n" +
            "OUTPUTS\n" +
            "analyticalModel - AnalyticalModel, one item. An updated copy carrying the commissioning record on the named zone. The supplied model is never modified.\n" +
            "commissioningData - PartF Commissioning Data, one item. The record as written.\n" +
            "\n" +
            "PART F BASIS\n" +
            "Paragraph 4.1 (page 31): mechanical ventilation systems must be commissioned to provide adequate ventilation, and a commissioning notice must be given to the building control body.\n" +
            "Paragraph 4.2 (page 31): air flow rates for mechanical ventilation in NEW dwellings must be measured, and a notice of the measured rates must be given to the building control body.\n" +
            "Paragraph 4.3 (page 31): the air flow measurement test and commissioning sheets should include, as a minimum, everything in Part 3 of the Appendix C example sheet.\n" +
            "Paragraph 4.10 (page 33) sets the measurement conditions: a calibrated air flow device with a proprietary hood, of plus or minus 5% accuracy, calibrated within the last 12 months at a UKAS-accredited centre, with all intended background ventilators and other air transfer devices OPEN and all internal and external doors and windows CLOSED.\n" +
            "Appendix C paragraph C2 (page 42) sets the pass condition: \"If the measured rate for each fan is equal to or greater than the design value, then the system meets the design standard.\" Where any measured value is lower, the system is adjusted and all air flows are remeasured.\n" +
            "Paragraphs 4.13 to 4.17 (page 34): sufficient information about the system and its maintenance requirements must be given to the building owner, in a clear manner for a non-technical audience, including the design flow rates.\n" +
            "Paragraphs 4.18 and 4.19 (page 35): a Home User Guide is provided for a new dwelling, with a Ventilation section giving non-technical advice.\n" +
            "\n" +
            "COMMISSIONING\n" +
            "The record also carries the engineer's answers to the Part F requirements that no analytical model contains - that the system is designed to minimise noise, that filters are reachable, that the controls are local to the spaces they serve, that the ductwork was installed as designed, where the intake and exhaust sit, and that the occupier was given operating instructions. Supply the check's exact name in confirmedChecks_ to record it as confirmed.\n" +
            "A recorded confirmation can resolve a check the model could not decide. It cannot overturn one the calculation found FAILING: a calculated failure is arithmetic against the Approved Document, and a checkbox does not change arithmetic.\n" +
            "\n" +
            "LIMITATIONS\n" +
            "Dwelling totals only at this level. Per-terminal measured rates live on the terminals themselves, inside each space's Part F Space Data, and are compared fan by fan as Appendix C paragraph C2 requires.\n" +
            "This component records evidence. It does not verify it, and recording that a notice was given is not the same as giving it.\n" +
            "\n" +
            "NOTES\n" +
            "All flow rates are in litres per second. Dates are free text so a project's own convention survives a round trip unchanged.\n" +
            "The supplied AnalyticalModel is never modified; an updated copy is returned.\n" +
            "\n" +
            "EXAMPLE\n" +
            "Commissioning a studio flat: set _zoneName to 'Flat 1', measurementEquipment_ to the hood model and serial number, calibrationDate_ to its last UKAS calibration, measuredContinuousSupply_ to 31 and measuredContinuousExtract_ to 30.5, and commissioningNoticeGiven_ and airFlowRateNoticeGiven_ to true. Feed the updated model into SAMAnalytical.CheckPartFCompliance: the commissioning checks resolve, the measured totals appear alongside the design totals in the report, and any fan whose measured rate is below its design rate is reported as a failure under Appendix C paragraph C2.\n";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public SAMAnalyticalSetPartFCommissioningData()
          : base("SAMAnalytical.SetPartFCommissioningData", "SAMAnalytical.SetPartFCommissioningData",
              description,
              "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result =
                [
                    new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel containing the dwelling zone. REQUIRED, one item. The model you supply is not modified; an updated copy is returned.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_zoneName", NickName = "_zoneName", Description = "Name of the zone representing the dwelling this commissioning evidence belongs to. REQUIRED, one item.\n\nCommissioning is per dwelling, not per space or per terminal: one Appendix C sheet covers one installation address. Per-terminal measured rates live on the terminals themselves.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                ];

                result.Add(Text("commissioningDate_", "Date of commissioning, per Appendix C section 3.5 (page 46). Free text, so a project's own date convention survives a round trip unchanged."));
                result.Add(Text("commissioningEngineer_", "Commissioning engineer, per Appendix C sections 1.4 and 3.5."));
                result.Add(Text("installationEngineer_", "Installation engineer, per Appendix C sections 1.3 and 2a.2."));
                result.Add(Text("measurementEquipment_", "Air flow measurement equipment used, model and serial number, per Appendix C section 3.1 (page 46).\n\nParagraph 4.10c requires a device with a proprietary hood attachment, an accuracy of plus or minus 5%, calibrated within the last 12 months at a UKAS-accredited centre."));
                result.Add(Text("calibrationDate_", "Date of the last UKAS calibration of that equipment, per Appendix C section 3.1 and paragraph 4.10c(iii). Paragraph 4.10 requires the calibration to be within the last 12 months."));

                result.Add(Number("measuredContinuousSupply_", "Measured total continuous SUPPLY across the dwelling, in litres per second.\n\nA measured value never overwrites a design value: Appendix C paragraph C2 compares the two, and the comparison needs both."));
                result.Add(Number("measuredContinuousExtract_", "Measured total continuous EXTRACT across the dwelling, in litres per second."));
                result.Add(Number("measuredHighSupply_", "Measured total HIGH RATE supply across the dwelling, in litres per second."));
                result.Add(Number("measuredHighExtract_", "Measured total HIGH RATE extract across the dwelling, in litres per second."));

                result.Add(Boolean("commissioningNoticeGiven_", "Whether the commissioning notice required by paragraph 4.1 (page 31) has been given to the building control body. Default: false."));
                result.Add(Boolean("airFlowRateNoticeGiven_", "Whether the notice of measured air flow rates required by paragraph 4.2 (page 31) has been given to the building control body, not later than five days after the final test. Default: false."));
                result.Add(Boolean("operatingAndMaintenanceIssued_", "Whether the operating and maintenance information of paragraphs 4.13 to 4.17 (page 34) has been issued to the building owner, including the design flow rates, the location and use of the controls, how and when to clean and maintain the system and its filters, and a copy of the completed Appendix C commissioning sheet. Default: false."));
                result.Add(Boolean("homeUserGuideIssued_", "Whether the Home User Guide of paragraphs 4.18 and 4.19 (page 35) has been provided, with a Ventilation section giving non-technical advice on the systems provided. Default: false."));

                global::Grasshopper.Kernel.Parameters.Param_String confirmedChecks = new() { Name = "confirmedChecks_", NickName = "confirmedChecks_", Description = "Names of Part F checks a person has confirmed, one per list item, exactly as they appear in the assessment report.\n\nThese are the requirements no analytical model contains - noise, maintenance access, controls, installation, terminal locations, intake and exhaust locations, operating and maintenance information. Each named check is recorded as User Confirmed.\n\nA confirmation can resolve a check the model could not decide. It CANNOT overturn one the calculation found failing.", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(confirmedChecks, ParamVisibility.Voluntary));

                result.Add(Text("confirmedBy_", "Who confirmed the checks named in confirmedChecks_. Recorded against each of them."));
                result.Add(Text("confirmedOn_", "When they confirmed them. Recorded against each of them. Free text."));
                result.Add(Text("notes_", "Any qualification the commissioning engineer wants carried with the record."));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                return
                [
                    new GH_SAMParam(new GooAnalyticalModelParam { Name = "analyticalModel", NickName = "analyticalModel", Description = "Updated copy of the AnalyticalModel, carrying the commissioning record on the named zone. The supplied model is left unchanged.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "commissioningData", NickName = "commissioningData", Description = "The commissioning record as written, so it can be inspected or reused.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                ];
            }
        }

        private static GH_SAMParam Text(string name, string description)
        {
            return new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = name, NickName = name, Description = description, Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary);
        }

        private static GH_SAMParam Number(string name, string description)
        {
            return new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = name, NickName = name, Description = description, Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary);
        }

        private static GH_SAMParam Boolean(string name, string description)
        {
            //Deliberately no SetPersistentData: with a persistent default, GetData always succeeds even when
            //the port is unconnected, so the read-time Boolean() helper below could never tell "explicitly
            //wired false" apart from "not wired at all" and its ?? fallback to the existing value could
            //never trigger. An unwired boolean here behaves exactly like the Text/Number inputs already do -
            //leaving whatever was previously recorded alone - which is what stops a rerun that only adds a
            //measured rate from silently resetting every previously-confirmed notice flag to false.
            global::Grasshopper.Kernel.Parameters.Param_Boolean result = new() { Name = name, NickName = name, Description = description, Access = GH_ParamAccess.item, Optional = true };

            return new GH_SAMParam(result, ParamVisibility.Voluntary);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel is null || analyticalModel.AdjacencyCluster is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_zoneName");
            string zoneName = null;
            if (!dataAccess.GetData(index, ref zoneName) || string.IsNullOrWhiteSpace(zoneName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = new(analyticalModel.AdjacencyCluster, deepClone: true);

            Zone zone = adjacencyCluster.GetZones()?.Find(x => x.Name == zoneName);
            if (zone is null)
            {
                List<Zone> zones = adjacencyCluster.GetZones() ?? [];

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("No zone named '{0}' is in the model. Zone names are case sensitive. Zones present: {1}.", zoneName, zones.Count == 0 ? "none" : string.Join(", ", zones.ConvertAll(x => x.Name))));
                return;
            }

            //A name that matches MORE than one zone is refused rather than resolved to the first: the
            //cluster order is arbitrary, and recording one dwelling's commissioning evidence on another
            //dwelling's zone would corrupt that dwelling's compliance result. Names are valid elsewhere
            //in the workflow (and handled by GUID there); this component keys on the name alone.
            if (adjacencyCluster.GetZones().FindAll(x => x.Name == zoneName).Count > 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("More than one zone is named '{0}', so the commissioning data cannot be attributed to one dwelling. Zone names must be unique for this component.", zoneName));
                return;
            }

            //Any record already on the zone is the starting point, so a definition that sets the design
            //stage answers and a later one that adds the measured rates do not erase each other.
            PartFCommissioningData partFCommissioningData = zone.GetValue<PartFCommissioningData>(ZoneParameter.PartFCommissioningData) is PartFCommissioningData partFCommissioningData_Existing
                ? new PartFCommissioningData(partFCommissioningData_Existing)
                : new PartFCommissioningData(zoneName);

            partFCommissioningData.DwellingName = zoneName;

            partFCommissioningData.CommissioningDate = Text("commissioningDate_", dataAccess) ?? partFCommissioningData.CommissioningDate;
            partFCommissioningData.CommissioningEngineer = Text("commissioningEngineer_", dataAccess) ?? partFCommissioningData.CommissioningEngineer;
            partFCommissioningData.InstallationEngineer = Text("installationEngineer_", dataAccess) ?? partFCommissioningData.InstallationEngineer;
            partFCommissioningData.MeasurementEquipment = Text("measurementEquipment_", dataAccess) ?? partFCommissioningData.MeasurementEquipment;
            partFCommissioningData.CalibrationDate = Text("calibrationDate_", dataAccess) ?? partFCommissioningData.CalibrationDate;
            partFCommissioningData.Notes = Text("notes_", dataAccess) ?? partFCommissioningData.Notes;

            partFCommissioningData.MeasuredContinuousSupplyTotal_Lps = Number("measuredContinuousSupply_", dataAccess) ?? partFCommissioningData.MeasuredContinuousSupplyTotal_Lps;
            partFCommissioningData.MeasuredContinuousExtractTotal_Lps = Number("measuredContinuousExtract_", dataAccess) ?? partFCommissioningData.MeasuredContinuousExtractTotal_Lps;
            partFCommissioningData.MeasuredHighSupplyTotal_Lps = Number("measuredHighSupply_", dataAccess) ?? partFCommissioningData.MeasuredHighSupplyTotal_Lps;
            partFCommissioningData.MeasuredHighExtractTotal_Lps = Number("measuredHighExtract_", dataAccess) ?? partFCommissioningData.MeasuredHighExtractTotal_Lps;

            partFCommissioningData.CommissioningNoticeGiven = Boolean("commissioningNoticeGiven_", dataAccess) ?? partFCommissioningData.CommissioningNoticeGiven;
            partFCommissioningData.AirFlowRateNoticeGiven = Boolean("airFlowRateNoticeGiven_", dataAccess) ?? partFCommissioningData.AirFlowRateNoticeGiven;
            partFCommissioningData.OperatingAndMaintenanceInformationIssued = Boolean("operatingAndMaintenanceIssued_", dataAccess) ?? partFCommissioningData.OperatingAndMaintenanceInformationIssued;
            partFCommissioningData.HomeUserGuideIssued = Boolean("homeUserGuideIssued_", dataAccess) ?? partFCommissioningData.HomeUserGuideIssued;

            //System classification, per Appendix C section 1.2. Only the mechanical ventilation with heat
            //recovery workflow is implemented, so the classification is stated rather than asked for.
            partFCommissioningData.SystemClassification ??= "Mechanical ventilation with heat recovery, as defined by Approved Document F";

            index = Params.IndexOfInputParam("confirmedChecks_");
            List<string> names_Check = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, names_Check);
            }

            string confirmedBy = Text("confirmedBy_", dataAccess);
            string confirmedOn = Text("confirmedOn_", dataAccess);

            foreach (string name_Check in names_Check ?? [])
            {
                if (string.IsNullOrWhiteSpace(name_Check))
                {
                    continue;
                }

                PartFComplianceCheck check = partFCommissioningData.InstallationChecks.Find(x => x is not null && string.Equals(x.Name, name_Check, StringComparison.Ordinal));
                if (check is null)
                {
                    check = new PartFComplianceCheck(name_Check, "Recorded confirmation", "Confirmed by the person named on this record.");
                    partFCommissioningData.InstallationChecks.Add(check);
                }

                check.Status = Enums.PartFComplianceStatus.UserConfirmed;
                check.ResponsiblePerson = confirmedBy ?? check.ResponsiblePerson;
                check.Date = confirmedOn ?? check.Date;
            }

            zone.SetValue(ZoneParameter.PartFCommissioningData, partFCommissioningData);
            adjacencyCluster.AddObject(zone);

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("commissioningData");
            if (index != -1)
            {
                dataAccess.SetData(index, partFCommissioningData);
            }
        }

        private string Text(string name, IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam(name);
            if (index == -1)
            {
                return null;
            }

            string result = null;

            //An empty input means "not supplied", so it leaves any existing value alone rather than
            //blanking a record an earlier component wrote.
            return dataAccess.GetData(index, ref result) && !string.IsNullOrWhiteSpace(result) ? result : null;
        }

        private double? Number(string name, IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam(name);
            if (index == -1)
            {
                return null;
            }

            double result = double.NaN;

            return dataAccess.GetData(index, ref result) && !double.IsNaN(result) && !double.IsInfinity(result) ? result : null;
        }

        private bool? Boolean(string name, IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam(name);
            if (index == -1)
            {
                return null;
            }

            bool result = false;

            return dataAccess.GetData(index, ref result) ? result : null;
        }
    }
}
