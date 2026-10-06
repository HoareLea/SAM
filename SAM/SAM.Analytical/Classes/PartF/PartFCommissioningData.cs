// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// Commissioning evidence for one dwelling, following Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England) Section 4 and the example sheet in Appendix C.
    /// <para>
    /// Paragraph 4.1 (page 31): mechanical ventilation systems must be commissioned and a commissioning
    /// notice given to the building control body. Paragraph 4.2: air flow rates for mechanical
    /// ventilation in new dwellings must be measured and a notice of the measured rates given. Paragraph
    /// 4.3: the commissioning sheet should include, as a minimum, everything in Part 3 of Appendix C.
    /// </para>
    /// <para>
    /// Paragraph 4.10 (page 33) sets the measurement conditions: a calibrated air flow device with a
    /// proprietary hood, accuracy of plus or minus 5%, calibrated within the last 12 months at a
    /// UKAS-accredited centre, with all transfer devices open and all internal and external doors and
    /// windows closed.
    /// </para>
    /// <para>
    /// Appendix C paragraph C2 (page 42) sets the pass condition: "If the measured rate for each fan is
    /// equal to or greater than the design value, then the system meets the design standard." Measured
    /// values are therefore compared with design values and never written over them - a design rate that
    /// silently became a measured rate would destroy the only evidence the comparison rests on.
    /// </para>
    /// </summary>
    public class PartFCommissioningData : SAMObject
    {
        /// <summary>
        /// Measurement accuracy [-] required of the air flow device by paragraph 4.10c(ii) (page 33),
        /// expressed as a fraction: plus or minus 5%.
        /// </summary>
        public const double RequiredMeasurementAccuracy = 0.05;

        public PartFCommissioningData()
        {
        }

        public PartFCommissioningData(string name)
            : base(name)
        {
        }

        public PartFCommissioningData(PartFCommissioningData partFCommissioningData)
            : base(partFCommissioningData)
        {
            if (partFCommissioningData is not null)
            {
                DwellingName = partFCommissioningData.DwellingName;
                MeasurementEquipment = partFCommissioningData.MeasurementEquipment;
                CalibrationDate = partFCommissioningData.CalibrationDate;
                CommissioningDate = partFCommissioningData.CommissioningDate;
                CommissioningEngineer = partFCommissioningData.CommissioningEngineer;
                InstallationEngineer = partFCommissioningData.InstallationEngineer;
                SystemClassification = partFCommissioningData.SystemClassification;
                MeasuredContinuousSupplyTotal_Lps = partFCommissioningData.MeasuredContinuousSupplyTotal_Lps;
                MeasuredContinuousExtractTotal_Lps = partFCommissioningData.MeasuredContinuousExtractTotal_Lps;
                MeasuredHighSupplyTotal_Lps = partFCommissioningData.MeasuredHighSupplyTotal_Lps;
                MeasuredHighExtractTotal_Lps = partFCommissioningData.MeasuredHighExtractTotal_Lps;
                OperatingAndMaintenanceInformationIssued = partFCommissioningData.OperatingAndMaintenanceInformationIssued;
                HomeUserGuideIssued = partFCommissioningData.HomeUserGuideIssued;
                CommissioningNoticeGiven = partFCommissioningData.CommissioningNoticeGiven;
                AirFlowRateNoticeGiven = partFCommissioningData.AirFlowRateNoticeGiven;
                Notes = partFCommissioningData.Notes;

                foreach (PartFComplianceCheck partFComplianceCheck in partFCommissioningData.InstallationChecks ?? [])
                {
                    InstallationChecks.Add(new PartFComplianceCheck(partFComplianceCheck));
                }
            }
        }

        public PartFCommissioningData(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>Name of the dwelling this evidence belongs to, or null in single dwelling mode.</summary>
        public string DwellingName { get; set; }

        /// <summary>
        /// Air flow measurement equipment used, model and serial number, per Appendix C section 3.1
        /// (page 46).
        /// </summary>
        public string MeasurementEquipment { get; set; }

        /// <summary>
        /// Date of the last UKAS calibration of that equipment, per Appendix C section 3.1 and paragraph
        /// 4.10c(iii). Free text so a project's own date convention survives a round trip.
        /// </summary>
        public string CalibrationDate { get; set; }

        /// <summary>Date of commissioning, per Appendix C section 3.5 (page 46).</summary>
        public string CommissioningDate { get; set; }

        /// <summary>Commissioning engineer, per Appendix C sections 1.4 and 3.5.</summary>
        public string CommissioningEngineer { get; set; }

        /// <summary>Installation engineer, per Appendix C sections 1.3 and 2a.2.</summary>
        public string InstallationEngineer { get; set; }

        /// <summary>
        /// System classification recorded on the sheet, per Appendix C section 1.2 (page 43). For the
        /// supported workflow this is mechanical ventilation with heat recovery, "as defined by Approved
        /// Document F".
        /// </summary>
        public string SystemClassification { get; set; }

        /// <summary>Measured total continuous supply [l/s] across the dwelling.</summary>
        public double? MeasuredContinuousSupplyTotal_Lps { get; set; }

        /// <summary>Measured total continuous extract [l/s] across the dwelling.</summary>
        public double? MeasuredContinuousExtractTotal_Lps { get; set; }

        /// <summary>Measured total high rate supply [l/s] across the dwelling.</summary>
        public double? MeasuredHighSupplyTotal_Lps { get; set; }

        /// <summary>Measured total high rate extract [l/s] across the dwelling.</summary>
        public double? MeasuredHighExtractTotal_Lps { get; set; }

        /// <summary>
        /// True where the operating and maintenance information of paragraphs 4.13 to 4.17 (page 34) has
        /// been issued to the building owner.
        /// </summary>
        public bool OperatingAndMaintenanceInformationIssued { get; set; }

        /// <summary>
        /// True where the Home User Guide of paragraphs 4.18 and 4.19 (page 35) has been provided.
        /// </summary>
        public bool HomeUserGuideIssued { get; set; }

        /// <summary>
        /// True where the commissioning notice required by paragraph 4.1 has been given to the building
        /// control body.
        /// </summary>
        public bool CommissioningNoticeGiven { get; set; }

        /// <summary>
        /// True where the notice of measured air flow rates required by paragraph 4.2 has been given to
        /// the building control body.
        /// </summary>
        public bool AirFlowRateNoticeGiven { get; set; }

        /// <summary>
        /// Installation and visual inspection checks from Appendix C Parts 2a and 2b, held as structured
        /// checks so each carries its own status, evidence, date and responsible person.
        /// </summary>
        public List<PartFComplianceCheck> InstallationChecks { get; set; } = [];

        /// <summary>Any qualification the commissioning engineer wants carried with the record.</summary>
        public string Notes { get; set; }

        /// <summary>
        /// True where any measured value at all has been recorded, so an assessment can tell "not
        /// commissioned yet" apart from "commissioned and failing".
        /// </summary>
        public bool HasMeasuredValues
        {
            get
            {
                return MeasuredContinuousSupplyTotal_Lps is not null
                    || MeasuredContinuousExtractTotal_Lps is not null
                    || MeasuredHighSupplyTotal_Lps is not null
                    || MeasuredHighExtractTotal_Lps is not null;
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            DwellingName = PartFJson.String(jsonObject, "DwellingName");
            MeasurementEquipment = PartFJson.String(jsonObject, "MeasurementEquipment");
            CalibrationDate = PartFJson.String(jsonObject, "CalibrationDate");
            CommissioningDate = PartFJson.String(jsonObject, "CommissioningDate");
            CommissioningEngineer = PartFJson.String(jsonObject, "CommissioningEngineer");
            InstallationEngineer = PartFJson.String(jsonObject, "InstallationEngineer");
            SystemClassification = PartFJson.String(jsonObject, "SystemClassification");
            Notes = PartFJson.String(jsonObject, "Notes");

            MeasuredContinuousSupplyTotal_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredContinuousSupplyTotal_Lps");
            MeasuredContinuousExtractTotal_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredContinuousExtractTotal_Lps");
            MeasuredHighSupplyTotal_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredHighSupplyTotal_Lps");
            MeasuredHighExtractTotal_Lps = PartFJson.NullableDouble(jsonObject, "MeasuredHighExtractTotal_Lps");

            OperatingAndMaintenanceInformationIssued = PartFJson.Boolean(jsonObject, "OperatingAndMaintenanceInformationIssued");
            HomeUserGuideIssued = PartFJson.Boolean(jsonObject, "HomeUserGuideIssued");
            CommissioningNoticeGiven = PartFJson.Boolean(jsonObject, "CommissioningNoticeGiven");
            AirFlowRateNoticeGiven = PartFJson.Boolean(jsonObject, "AirFlowRateNoticeGiven");

            InstallationChecks = [];
            if (jsonObject["InstallationChecks"] is JsonArray jsonArray)
            {
                foreach (JsonNode jsonNode in jsonArray)
                {
                    if (jsonNode is JsonObject jsonObject_Check)
                    {
                        InstallationChecks.Add(new PartFComplianceCheck(jsonObject_Check));
                    }
                }
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            PartFJson.SetString(result, "DwellingName", DwellingName);
            PartFJson.SetString(result, "MeasurementEquipment", MeasurementEquipment);
            PartFJson.SetString(result, "CalibrationDate", CalibrationDate);
            PartFJson.SetString(result, "CommissioningDate", CommissioningDate);
            PartFJson.SetString(result, "CommissioningEngineer", CommissioningEngineer);
            PartFJson.SetString(result, "InstallationEngineer", InstallationEngineer);
            PartFJson.SetString(result, "SystemClassification", SystemClassification);
            PartFJson.SetString(result, "Notes", Notes);

            PartFJson.SetNullableDouble(result, "MeasuredContinuousSupplyTotal_Lps", MeasuredContinuousSupplyTotal_Lps);
            PartFJson.SetNullableDouble(result, "MeasuredContinuousExtractTotal_Lps", MeasuredContinuousExtractTotal_Lps);
            PartFJson.SetNullableDouble(result, "MeasuredHighSupplyTotal_Lps", MeasuredHighSupplyTotal_Lps);
            PartFJson.SetNullableDouble(result, "MeasuredHighExtractTotal_Lps", MeasuredHighExtractTotal_Lps);

            result["OperatingAndMaintenanceInformationIssued"] = OperatingAndMaintenanceInformationIssued;
            result["HomeUserGuideIssued"] = HomeUserGuideIssued;
            result["CommissioningNoticeGiven"] = CommissioningNoticeGiven;
            result["AirFlowRateNoticeGiven"] = AirFlowRateNoticeGiven;

            JsonArray jsonArray = [];
            foreach (PartFComplianceCheck partFComplianceCheck in InstallationChecks ?? [])
            {
                JsonObject jsonObject_Check = partFComplianceCheck?.ToJsonObject();
                if (jsonObject_Check is not null)
                {
                    jsonArray.Add(jsonObject_Check);
                }
            }

            result["InstallationChecks"] = jsonArray;

            return result;
        }
    }
}
