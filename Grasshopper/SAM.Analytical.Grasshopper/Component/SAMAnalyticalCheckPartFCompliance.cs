// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Enums;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Runs the Approved Document F conformance assessment over an AnalyticalModel and returns the report,
    /// the airflow schematic and the clause-level outcomes.
    /// </summary>
    public class SAMAnalyticalCheckPartFCompliance : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new("3f8c1f2b-9a41-4d0e-8b57-2c6a41f7d913");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        private const string description =
            "SUMMARY\n" +
            "Assesses a NEW dwelling against Approved Document F - Ventilation, Volume 1: Dwellings (2021 edition, for use in England, in effect from 15 June 2022) and returns the engineering report, the compact airflow schematic and the clause-level outcomes. It sizes the model exactly as SAMAnalytical.AddVentilationPropertiesByPartF does, then goes further: it builds the internal transfer-air network, assesses every internal door against the paragraph 1.25 free-area requirement, assesses purge ventilation in every habitable room, reads any commissioning evidence held on the dwelling zones, and resolves an overall conformance-assessment status.\n" +
            "This is an ASSESSMENT, not a certificate. Software cannot certify compliance with the Building Regulations. What the component reports is which requirements were calculated, which were verified from the model geometry, which a person confirmed, and which remain open.\n" +
            "\n" +
            "INPUTS\n" +
            "_analyticalModel - AnalyticalModel. Required, one model. Each space should carry a positive Area in m2 and Volume in m3, and be named so the shared space-use classification recognises it. Internal partitions must be related to the spaces on BOTH sides, because adjacency is what the transfer-air network is built from.\n" +
            "zoneCategoryName_ - text, optional, no default. Zone category containing zones that represent individual flats or dwellings. Leave empty when the complete AnalyticalModel represents one house.\n" +
            "setbackFlowRateFactor_ - number, optional, default 0.30, valid range greater than 0 and no greater than 1.\n" +
            "extractAllocationStrategy_ - text, optional, default MinimumFirstCookingPriority. How continuous extract ABOVE the Table 1.2 minimums is shared between the extract terminals. MinimumFirstCookingPriority sends it to the local kitchen extract; VolumeWeighted shares it by room volume, reproducing the pre-terminal SAM behaviour.\n" +
            "operatingMode_ - text, optional, default ContinuousDesign. Which condition the report and schematic are drawn at: ContinuousDesign, HighBoost, Setback or MeasuredCommissioning.\n" +
            "\n" +
            "OUTPUTS\n" +
            "analyticalModel - AnalyticalModel, one item. An updated copy; the supplied model is never modified. Every classified space carries its Part F Space Data including the terminal collection and its purge record, and every internal door aperture carries its Part F Door Transfer Data.\n" +
            "report - text, one item. The complete Part F conformance assessment for every dwelling, beginning with the assumptions and containing the dwelling summary, the airflow schematic, the terminal schedules, the transfer-air and door-undercut schedules, the purge assessment, the commissioning status, the warnings, the failed, unresolved and engineering-review checks, the regulatory references and the overall status.\n" +
            "schematic - text, list, one per dwelling. The compact airflow schematic on its own, for the selected operating mode.\n" +
            "status - text, list, one per dwelling. The overall conformance-assessment status: Pass, Fail, Partial, Cannot Be Determined, Engineering Review Required or Not Assessed.\n" +
            "failed - text, list. Every failed check across every dwelling, prefixed with the dwelling name.\n" +
            "unresolved - text, list. Every check that could not be determined, and every check needing engineering review, prefixed with the dwelling name. These are the requirements a person still has to answer.\n" +
            "\n" +
            "PART F BASIS\n" +
            "Approved Document F - Ventilation, Volume 1: Dwellings, 2021 edition, for use in England. Requirement F1: Means of ventilation. Scope: NEW DWELLINGS ONLY, mechanical ventilation with heat recovery (paragraphs 1.67 to 1.73) only.\n" +
            "\n" +
            "TERMINAL CALCULATION\n" +
            "The unit of the calculation is the TERMINAL, not the room, because one room can require more than one. Appendix A (page 36) makes a studio and an open-plan living-kitchen habitable rooms, because neither is SOLELY a kitchen, so paragraph 1.67 requires mechanical supply to them; and both contain the cooking function, so paragraph 1.17a and Table 1.2 require kitchen extract from them as well. Both terminals are established and sized.\n" +
            "Continuous and high rates are separate conditions and are never combined. The continuous design rate is the Approved Document F sizing case: the greater of the bedroom or one-habitable-room rate and the floor-area rate. Table 1.2 sets two separate things - the TOTAL of continuous extract reaches the whole dwelling rate, and EACH room reaches its own minimum high rate - so the sum of the per-room minimums never raises the continuous rate. Each terminal's high rate is the greater of its continuous rate and its Table 1.2 minimum - Table 1.2 note 1: where the continuous rate is already at or above the minimum high rate, no extra ventilation is needed. High supply balances total high extract.\n" +
            "The setback rate is a SAM reduced-operation convention, NOT a Part F condition. It is applied only after every regulatory minimum has been established, and is never checked against Table 1.2.\n" +
            "\n" +
            "LOCAL KITCHEN EXTRACT\n" +
            "Local kitchen extract is a terminal role of its own, separate from general wet-room extract. Extract from a bathroom or ensuite may balance the dwelling airflow but is not local kitchen extract and never satisfies paragraph 1.17a.\n" +
            "How the extract is actually provided is an engineering input, held on the space as the PartF Local Extract Method parameter, because the analytical model looks identical whether the hob has an MVHR extract terminal over it, a cooker hood ducted outside, or a recirculating hood. MVHRContinuousTerminal is assessed against Table 1.2 and counted in the balanced continuous flow. CookerHoodExtractingOutside is assessed against Table 1.1 at 30 l/s intermittent and is deliberately NOT counted in the balanced continuous flow. SeparateIntermittentExtract is assessed against Table 1.1 at 60 l/s, also outside the balanced flow. RecirculatingCookerHood FAILS: Diagram 1.2 note 1 states a recirculating cooker hood on its own does not provide a means of ventilation that complies with Part F. NotRepresented fails and names the missing provision.\n" +
            "With nothing recorded, a continuous MVHR terminal is assumed - that being what the rest of the system implies - and reported as needing confirmation that it discharges to outside, never passed silently.\n" +
            "\n" +
            "TRANSFER AIR\n" +
            "Every space carries a net airflow: supply less extract. Air is routed from each net-supply space to each net-extract space in proportion to what each has to give and take, along the shortest connected path, and the contributions are summed on each internal route.\n" +
            "Where the dwelling's internal connections form a TREE, this reproduces the one answer conservation of air flow allows, exactly, and every route is reported as Uniquely Determined - no engineering choice was involved. Where they contain a LOOP there is genuinely more than one valid answer, because Approved Document F does not say how air divides between parallel paths; the same allocation is applied, the total is still correct, and every route is reported as Calculated Using Allocation Strategy so the engineer knows the split is a design decision they may override.\n" +
            "The network is built from the dwelling's internal SEPARATING ELEMENTS, not from its doors: a model frequently carries the partition between a studio and its bathroom without carrying a door aperture in it, and treating a missing aperture as a missing adjacency would report a dwelling as disconnected when it is only under-modelled. An element with one adjacent space is external. A connection to a space outside the dwelling - a communal corridor, a neighbouring flat, an excluded zone - is never a route, so no dwelling's transfer air can cross into another's.\n" +
            "\n" +
            "DOOR UNDERCUTS\n" +
            "Paragraph 1.25 (page 10): internal doors should allow air to flow through the dwelling by providing a minimum free area equivalent to a 10mm undercut in a 760mm wide door - that is 7,600mm2 - undercut to 10mm above a fitted floor finish or 20mm above an unfinished floor surface.\n" +
            "EVERY internal door within the dwelling is assessed, not only the doors the flow solver loads. The free area is the requirement; the two undercut heights are the datum it is measured from, and an undercut is converted to an area before being judged. A transfer grille, a permanent opening or an open passage of at least the equivalent free area serves the same purpose.\n" +
            "An analytical model does not represent the gap under a door leaf, and a door's modelled height is not evidence of one, so the provided value is always an engineering input on the Part F Door Transfer Data parameter. Its absence is reported as Cannot Be Determined and is NEVER treated as compliance.\n" +
            "\n" +
            "PURGE VENTILATION\n" +
            "Paragraphs 1.26 to 1.31 and Table 1.4 (page 11). Every habitable room needs purge ventilation capable of at least four air changes per hour directly to the outside, through openings meeting the Table 1.4 minimum areas or through a mechanical extract system.\n" +
            "The requirement is calculated from the room volume and floor area. The OPENABLE area is an engineering input, because Table 1.4 is about the area of the opening and a fixed light adds window area while opening nothing; the window area the model carries is reported as context and never used as the openable area. The Table 1.4 row depends on the opening type and angle, which are product properties, so with neither recorded the required area is reported as unknown rather than defaulted to the more permissive 1/20.\n" +
            "Paragraph 0.21 (page 4): Approved Document O may require a higher purge standard, and where it does the higher applies. That interaction is reported, not calculated here.\n" +
            "\n" +
            "COMMISSIONING\n" +
            "Section 4 and Appendix C. Commissioning evidence is read from the PartF Commissioning Data parameter on each dwelling zone. Design and measured values are held separately and a measured value NEVER overwrites a design value. Appendix C paragraph C2 sets the pass condition: the measured rate for each fan must be equal to or greater than its design value.\n" +
            "The same record carries the engineer's answers to the requirements no analytical model contains - noise, maintenance access, controls, installation, intake and exhaust locations, operating and maintenance information. A recorded answer can resolve a check the model could not decide; it cannot overturn one the calculation found failing, because a failure here is arithmetic against the Approved Document.\n" +
            "\n" +
            "LIMITATIONS\n" +
            "England only. New dwellings only: Section 3, work on existing dwellings, is not assessed.\n" +
            "Mechanical ventilation with heat recovery only. Natural ventilation with background ventilators and intermittent extract fans, and continuous mechanical extract ventilation, are not implemented as system types, although Table 1.1 rates are applied to an individual intermittent device such as a cooker hood.\n" +
            "Not assessed from geometry, and reported as open checks a person resolves: extract terminal heights (paragraph 1.20), cooker hood height above the hob (paragraph 1.21), supply terminal direction (paragraph 1.68), recirculation of moist air (paragraph 1.71), background ventilators (paragraph 1.72), noise (paragraphs 1.5 to 1.7), maintenance access (paragraph 1.8), controls (paragraphs 1.33 to 1.37), installation and ductwork (paragraphs 1.74 to 1.83), and outdoor air intake and exhaust locations (Section 2).\n" +
            "Not implemented at all: background ventilator equivalent areas (Table 1.7), basements (paragraphs 1.38 to 1.41), and ventilation of a habitable room through another room (paragraphs 1.42 to 1.44), which is reported where an internal habitable room is found but not sized.\n" +
            "A dwelling with several spaces and no internal separating element between any two of them cannot have its transfer air assessed at all. That is reported as a gap in the model rather than as a pass or a fail.\n" +
            "A 2026 edition of Approved Document F, Volume 1 has been published, taking effect on 24 March 2027. This component implements the 2021 edition only and does not combine requirements from the two.\n" +
            "\n" +
            "NOTES\n" +
            "Areas are in square metres, volumes in cubic metres and all flow rates in litres per second. No unit conversion is applied and rates are not rounded.\n" +
            "A dwelling can never reach an overall Pass while any mandatory check is failed or unresolved. A model with nothing confirmed will report Cannot Be Determined, which is the honest answer: reporting silence as compliance would be worse than reporting nothing.\n" +
            "The report and schematic are generated with no user-interface dependency, so the same text appears here, in SAM_UI and in the regression tests.\n" +
            "The supplied AnalyticalModel is never modified; an updated copy is returned.\n" +
            "\n" +
            "EXAMPLE\n" +
            "Studio flat: a 75 m2 studio containing the cooking function and a 25 m2 bathroom, connected by one internal door, 100 m2 in total. One habitable room, so Table 1.3 note 1 gives 13 l/s; the floor-area rate gives 0.3 x 100 = 30 l/s. The continuous design rate is therefore 30 l/s, and the Table 1.2 per-room high-rate minimums of kitchen 13 + bathroom 8 = 21 l/s are reported without raising it. Every terminal takes its minimum and the remaining 9 l/s goes to the local kitchen extract, so the studio is supplied 30 l/s and extracts 22 l/s, and the bathroom extracts 8 l/s. The studio is then +8 l/s net and the bathroom -8 l/s, so 8 l/s of transfer air crosses the internal door, and the schematic reads:\n" +
            "  Outdoor supply\n" +
            "        v\n" +
            "  Studio: +30 l/s supply, -22 l/s local kitchen extract\n" +
            "        |\n" +
            "        \\---- 8 l/s through internal door ----> Bathroom: -8 l/s extract\n" +
            "With no undercut recorded on that door and no commissioning evidence, the dwelling's overall status is Cannot Be Determined, and the unresolved list names exactly what is outstanding.\n";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public SAMAnalyticalCheckPartFCompliance()
          : base("SAMAnalytical.CheckPartFCompliance", "SAMAnalytical.CheckPartFCompliance",
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
                    new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel. REQUIRED, one item.\n\nApplies to NEW dwellings in England only.\n\nEach space should carry an Area parameter in m2 and a Volume parameter in m3, both positive, and be named so the shared space use classification recognises it.\n\nInternal partitions must be RELATED TO THE SPACES ON BOTH SIDES, because adjacency, not the presence of a door aperture, is what the transfer air network is built from. A dwelling of several spaces with no internal separating element between any two of them cannot have its transfer air assessed, and that is reported as a gap in the model.\n\nThe model you supply is not modified; an updated copy is returned.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Optional zone category containing zones that represent individual flats or dwellings.\n\nText, one item, OPTIONAL, no default.\n\nEMPTY: the complete model is assessed as one new dwelling, which is the normal single house workflow and is never reported as a problem.\n\nSUPPLIED: each zone in the category marked Is Dwelling = true is assessed independently. No dwelling's transfer air can cross into another's, and communal areas are excluded and reported.", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding),
                ];

                global::Grasshopper.Kernel.Parameters.Param_Number number = new() { Name = "setbackFlowRateFactor_", NickName = "setbackFlowRateFactor_", Description = "Optional setback operating-rate factor applied to the continuous design flow rates. Default: 0.30. Valid range: greater than 0 and no greater than 1.\n\nA SAM reduced-operation convention, not a regulatory rate. Neither the 2021 nor the 2026 edition of Approved Document F specifies a reduced operating rate for mechanical ventilation with heat recovery. Setback rates are not checked against the Table 1.2 minimums.", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(PartFData.DefaultSetbackFlowRateFactor);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_String strategy = new() { Name = "extractAllocationStrategy_", NickName = "extractAllocationStrategy_", Description = "Optional. How continuous extract ABOVE the Table 1.2 minimums is shared between the extract terminals. Default: MinimumFirstCookingPriority.\n\nApproved Document F fixes only two things about extract totals: each wet room reaches at least its Table 1.2 minimum high rate (paragraph 1.70), and the sum of all extract on its continuous rate reaches the whole dwelling ventilation rate (Table 1.2, continuous rate column). The split of the surplus is an ENGINEERING STRATEGY, not a regulatory value.\n\nMinimumFirstCookingPriority: every terminal takes its Table 1.2 minimum, then all remaining continuous extract goes to the local kitchen extract. The cooking function is the dwelling's largest single source of moisture and cooking pollutants, and removing them closest to source is the stated aim of extract ventilation in requirement F1(1)(a).\n\nVolumeWeighted: the surplus is shared between the extract terminals in proportion to room volume. Reproduces the split SAM produced before terminal-level sizing existed.", Access = GH_ParamAccess.item, Optional = true };
                strategy.SetPersistentData(PartFExtractAllocationStrategy.MinimumFirstCookingPriority.ToString());
                result.Add(new GH_SAMParam(strategy, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_String mode = new() { Name = "operatingMode_", NickName = "operatingMode_", Description = "Optional. Which operating condition the report and schematic are drawn at. Default: ContinuousDesign.\n\nContinuousDesign - the Approved Document F sizing condition. Select equipment on this.\nHighBoost - the Table 1.2 high rate condition, for when additional extraction is required.\nSetback - the SAM reduced-operation convention. Not a Part F condition.\nMeasuredCommissioning - the rates recorded on site under Section 4 and Appendix C Part 3.\n\nThe conditions are never combined: a single number that was partly design, partly boost and partly setback would describe an operating state the system never enters.", Access = GH_ParamAccess.item, Optional = true };
                mode.SetPersistentData(PartFOperatingMode.ContinuousDesign.ToString());
                result.Add(new GH_SAMParam(mode, ParamVisibility.Voluntary));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                return
                [
                    new GH_SAMParam(new GooAnalyticalModelParam { Name = "analyticalModel", NickName = "analyticalModel", Description = "Updated copy of the AnalyticalModel. The supplied model is left unchanged.\n\nEvery classified space carries its Part F Space Data, including the TERMINAL COLLECTION and the purge record. Every internal door aperture carries its Part F Door Transfer Data, holding both the paragraph 1.25 requirement and any engineering input a person supplied, which the next calculation reads back rather than overwriting.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "report", NickName = "report", Description = "The complete Part F conformance assessment, one item, as plain text.\n\nBegins with the assumptions, then for every dwelling: the airflow schematic, the dwelling summary and governing calculation, the supply, general-extract and local-kitchen-extract schedules, the internal transfer-air schedule, the door-undercut and free-area schedule, the purge assessment, the commissioning status, the warnings and notes, the failed, unresolved and engineering-review checks, the regulatory references and the overall status.\n\nIt is an assessment, not a certificate.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "schematic", NickName = "schematic", Description = "The compact airflow schematic of each dwelling, one item per dwelling, at the selected operating mode. Shows outdoor supply, each space's supply and extract, and the transfer air crossing each internal door on the way to an extract terminal.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "status", NickName = "status", Description = "The overall Part F conformance-assessment status of each dwelling, one item per dwelling, matching the schematic output item for item.\n\nPass, Fail, Partial, Cannot Be Determined, Engineering Review Required or Not Assessed. A dwelling can never reach Pass while any mandatory check is failed or unresolved.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "failed", NickName = "failed", Description = "Every failed check across every dwelling, prefixed with the dwelling name. Each names the requirement, the Approved Document paragraph it comes from, and the evidence the conclusion rests on.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "unresolved", NickName = "unresolved", Description = "Every check that could not be determined from the model, and every check needing an engineering decision, prefixed with the dwelling name.\n\nThese are the requirements a person still has to answer - noise, maintenance access, controls, installation, terminal locations, door undercuts, openable areas, commissioning. They are never quietly passed, and each holds the dwelling off an overall pass until it is resolved.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                ];
            }
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

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            if (partFCalculator is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not load PartF Calculator");
                return;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            string zoneCategoryName = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref zoneCategoryName);
            }

            index = Params.IndexOfInputParam("setbackFlowRateFactor_");
            if (index != -1)
            {
                double setbackFlowRateFactor = double.NaN;
                if (dataAccess.GetData(index, ref setbackFlowRateFactor))
                {
                    if (PartFData.IsValidSetbackFlowRateFactor(setbackFlowRateFactor))
                    {
                        partFCalculator.SetbackFlowRateFactor = setbackFlowRateFactor;
                    }
                    else
                    {
                        //Reported rather than silently substituted: a factor above 1 would give a setback
                        //rate above the continuous design rate, and NaN would poison every rate.
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("setbackFlowRateFactor_ must be greater than 0 and no greater than 1. '{0}' was ignored and the default {1} used instead.", setbackFlowRateFactor, PartFData.DefaultSetbackFlowRateFactor));
                    }
                }
            }

            index = Params.IndexOfInputParam("extractAllocationStrategy_");
            if (index != -1)
            {
                string text = null;
                if (dataAccess.GetData(index, ref text) && !string.IsNullOrWhiteSpace(text))
                {
                    //Enum.TryParse alone is not enough: given numeric text such as "9" it succeeds and
                    //returns that value even though no PartFExtractAllocationStrategy member has it, so
                    //IsDefined is checked as well (the same guard the operatingMode_ branch below uses).
                    if (Enum.TryParse(text, true, out PartFExtractAllocationStrategy partFExtractAllocationStrategy) && Enum.IsDefined(typeof(PartFExtractAllocationStrategy), partFExtractAllocationStrategy))
                    {
                        partFCalculator.ExtractAllocationStrategy = partFExtractAllocationStrategy;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("'{0}' is not a recognised extract allocation strategy. Use MinimumFirstCookingPriority or VolumeWeighted. The rule set's own strategy has been used instead.", text));
                    }
                }
            }

            PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign;
            index = Params.IndexOfInputParam("operatingMode_");
            if (index != -1)
            {
                string text = null;
                if (dataAccess.GetData(index, ref text) && !string.IsNullOrWhiteSpace(text))
                {
                    //Enum.TryParse alone is not enough: given numeric text such as "9" it succeeds and
                    //returns that value even though no PartFOperatingMode member has it, so IsDefined is
                    //checked as well. Without it, an undefined value reaches PartFSchematic.Rate, whose
                    //default branch returns null - producing a misleading empty schematic under a heading
                    //naming the raw number rather than refusing the bad input up front.
                    if (!Enum.TryParse(text, true, out partFOperatingMode) || !Enum.IsDefined(typeof(PartFOperatingMode), partFOperatingMode))
                    {
                        partFOperatingMode = PartFOperatingMode.ContinuousDesign;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("'{0}' is not a recognised operating mode. Use ContinuousDesign, HighBoost, Setback or MeasuredCommissioning. ContinuousDesign has been used instead.", text));
                    }
                }
            }

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            partFCalculator.Calculate(zoneCategoryName);

            analyticalModel = new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster);

            foreach (string remark in partFCalculator.Remarks)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, remark);
            }

            foreach (string warning in partFCalculator.Warnings)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }

            List<string> schematics = [];
            List<string> statuses = [];
            List<string> failed = [];
            List<string> unresolved = [];

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;
                if (complianceResult is null)
                {
                    continue;
                }

                string prefix = string.IsNullOrWhiteSpace(dwellingResult.Name) ? string.Empty : dwellingResult.Name + ": ";

                schematics.Add(PartFSchematic.Build(complianceResult, partFOperatingMode));
                statuses.Add(Core.Query.Description(complianceResult.OverallStatus));

                foreach (PartFComplianceCheck check in complianceResult.FailedChecks)
                {
                    failed.Add(string.Format("{0}{1} [{2}] {3}", prefix, check.Name, check.SourceReference, check.Evidence));
                }

                //Both kinds of open check go to one list: from the engineer's point of view they are the
                //same job - something that still has to be answered before the dwelling can pass.
                foreach (PartFComplianceCheck check in complianceResult.UnresolvedChecks)
                {
                    unresolved.Add(string.Format("{0}{1} [{2}] {3}", prefix, check.Name, check.SourceReference, check.Evidence));
                }

                foreach (PartFComplianceCheck check in complianceResult.EngineeringReviewChecks)
                {
                    unresolved.Add(string.Format("{0}ENGINEERING REVIEW: {1} [{2}] {3}", prefix, check.Name, check.SourceReference, check.Evidence));
                }
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("report");
            if (index != -1)
            {
                dataAccess.SetData(index, PartFReport.Build(partFCalculator, partFOperatingMode));
            }

            index = Params.IndexOfOutputParam("schematic");
            if (index != -1)
            {
                dataAccess.SetDataList(index, schematics);
            }

            index = Params.IndexOfOutputParam("status");
            if (index != -1)
            {
                dataAccess.SetDataList(index, statuses);
            }

            index = Params.IndexOfOutputParam("failed");
            if (index != -1)
            {
                dataAccess.SetDataList(index, failed);
            }

            index = Params.IndexOfOutputParam("unresolved");
            if (index != -1)
            {
                dataAccess.SetDataList(index, unresolved);
            }
        }
    }
}
