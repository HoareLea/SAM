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
    /// Adds the internal transfer-air doors Approved Document F requires but the model does not carry.
    /// </summary>
    public class SAMAnalyticalAddTransferAirDoorsByPartF : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new("dd4e991a-40b4-4ecd-9629-9bd7e04e89fd");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        private const string description =
            "SUMMARY\n" +
            "Adds or updates the internal transfer-air doors required by Approved Document F - Ventilation, Volume 1: Dwellings (2021 edition, for use in England), paragraph 1.25 (page 10), after the Part F ventilation properties have been assigned. Existing suitable doors are reused - their paragraph 1.25 records are refreshed, never duplicated. Where a transfer path is required and no suitable door is present, ONE default internal door is created in the shared internal wall and its undercut requirement is recorded using the same Part F transfer-air methodology as SAMAnalytical.CheckPartFCompliance and the SAM_UI Part F assessment.\n" +
            "Use this component immediately after SAMAnalytical.AddVentilationPropertiesByPartF, before the simulation and compliance operations.\n" +
            "\n" +
            "INPUTS\n" +
            "_analyticalModel - AnalyticalModel. Required, one model. Normally the output of SAMAnalytical.AddVentilationPropertiesByPartF. Internal partitions must be related to the spaces on BOTH sides, because adjacency is what the transfer-air network is built from.\n" +
            "zoneCategoryName_ - text, optional, no default. Zone category containing the dwelling zones. Supply the SAME value as the upstream Part F component; leave empty when the complete AnalyticalModel represents one house.\n" +
            "setbackFlowRateFactor_ - number, optional, default 0.30, valid range greater than 0 and no greater than 1. Supply the SAME value as the upstream Part F component: this component re-runs the sizing to resolve the transfer paths, and a different factor would change the setback rates written on the spaces.\n" +
            "\n" +
            "OUTPUTS\n" +
            "analyticalModel - AnalyticalModel, one item. An updated copy; the supplied model is never modified. Carries the Part F sizing exactly as the upstream component wrote it, the refreshed Part F Door Transfer Data on every existing internal door, and one new door aperture with its record for every resolved transfer path.\n" +
            "doors - Aperture, list. The internal doors created by this run. Empty where every transfer path already had a modelled door.\n" +
            "notes - text, list. What the component did: each door created and the count of existing doors reused.\n" +
            "unresolved - text, list. Every route where transfer air is required but no defensible door could be created, with the reason: no shared internal wall, or no candidate wall that fits the door. These are the routes a person still has to resolve.\n" +
            "\n" +
            "WHAT IS CREATED\n" +
            "The door is 760 mm wide - the paragraph 1.25 reference door width, so a 10 mm undercut across it provides exactly the required 7,600 mm2 free area - and 2,100 mm high, a SAM modelling default, Approved Document F setting no door height. It stands on the bottom edge of the shared wall, centred on the clearest horizontal position: the panel centre where it is free, otherwise as close to it as the existing apertures allow. The default internal-door construction from the active aperture construction library is used where it provides one.\n" +
            "Where the two spaces share more than one wall panel that can take the door - typically one wall split into two panels by the modelling - the door goes into the panel the direct line between the two space locations passes through, the wall the two rooms actually meet through; where the geometry ties two candidate walls, the shorter wall wins, and where those are also the same length, the stable first candidate is taken.\n" +
            "The created door's Part F Door Transfer Data carries the paragraph 1.25 REQUIREMENT - minimum free area 7,600 mm2, 10 mm above a fitted floor finish or 20 mm above an unfinished floor surface - and the calculated transfer flow. The PROVIDED undercut is deliberately not recorded: an analytical model does not represent the gap under a door leaf, and absence of evidence is never compliance, so the route reports Cannot Be Determined until the engineer records what is actually provided.\n" +
            "\n" +
            "WHAT IS NOT CREATED\n" +
            "No door is created where no transfer air flows - two adjacent bedrooms, both supplied, share a partition that nothing needs to cross - and none where the route already has a modelled door, so re-running the component adds nothing.\n" +
            "No door is guessed into existence: where the spaces share no internal wall, or no candidate wall can fit the door, the route is reported in unresolved and left for the engineer.\n" +
            "\n" +
            "NOTES\n" +
            "The supplied AnalyticalModel is never modified; an updated copy is returned.\n" +
            "Using this component does not by itself demonstrate or guarantee compliance with Building Regulations Part F. Results must be checked by a suitably qualified engineer against the full Approved Document.\n" +
            "\n" +
            "EXAMPLE\n" +
            "AnalyticalModel to SAMAnalytical.AddVentilationPropertiesByPartF to SAMAnalytical.AddTransferAirDoorsByPartF to updated AnalyticalModel. A 75 m2 studio flat with a 25 m2 bathroom and an undetailed partition between them needs 8 l/s of transfer air across that partition: the component creates one 760 x 2100 mm internal door in it and records the paragraph 1.25 requirement. Running the component a second time creates nothing - the door now serves the route.";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public SAMAnalyticalAddTransferAirDoorsByPartF()
          : base("SAMAnalytical.AddTransferAirDoorsByPartF", "SAMAnalytical.AddTransferAirDoorsByPartF",
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
                    new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel. REQUIRED, one item. Normally the output of SAMAnalytical.AddVentilationPropertiesByPartF, so every space carries its Part F Space Data.\n\nInternal partitions must be RELATED TO THE SPACES ON BOTH SIDES, because adjacency, not the presence of a door aperture, is what the transfer air network is built from.\n\nThe model you supply is not modified; an updated copy is returned on the analyticalModel output.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Optional zone category containing zones that represent individual flats or dwellings. Supply the SAME value as the upstream SAMAnalytical.AddVentilationPropertiesByPartF, so the dwellings are grouped identically.\n\nText, one item, OPTIONAL, no default.\n\nEMPTY: the complete model is treated as one new dwelling, the normal single house workflow.\n\nSUPPLIED: each zone in the category marked Is Dwelling = true is processed independently, and no dwelling's transfer air can cross into another's.", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding),
                ];

                global::Grasshopper.Kernel.Parameters.Param_Number number = new() { Name = "setbackFlowRateFactor_", NickName = "setbackFlowRateFactor_", Description = "Optional setback operating-rate factor. Default: 0.30. Valid range: greater than 0 and no greater than 1.\n\nSupply the SAME value as the upstream SAMAnalytical.AddVentilationPropertiesByPartF. This component re-runs the Part F sizing to resolve the transfer paths, and a different factor here would change the setback rates written on the spaces.", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(PartFData.DefaultSetbackFlowRateFactor);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                return
                [
                    new GH_SAMParam(new GooAnalyticalModelParam { Name = "analyticalModel", NickName = "analyticalModel", Description = "Updated copy of the AnalyticalModel. AnalyticalModel, one item. The supplied model is left unchanged.\n\nCarries the Part F sizing exactly as the upstream component wrote it, the refreshed Part F Door Transfer Data on every existing internal door, and one new door aperture with its paragraph 1.25 record for every resolved transfer path.", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new GooApertureParam() { Name = "doors", NickName = "doors", Description = "The internal doors created by this run. Aperture, list. Empty where every transfer path already had a modelled door.\n\nEach is 760 x 2100 mm - the paragraph 1.25 reference width, so a 10 mm undercut across it is exactly the required free area - standing on the bottom edge of the shared wall, centred on its clearest length.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "notes", NickName = "notes", Description = "What the component did, one entry per action: each door created with its route and its paragraph 1.25 requirement, and the count of existing doors reused. Text, list.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "unresolved", NickName = "unresolved", Description = "Every route where transfer air is required but no defensible door could be created, with the reason - no shared internal wall between the spaces, or no candidate wall that can fit the door. Text, list.\n\nThese are the routes a person still has to resolve; nothing is guessed or silently approximated.", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                ];
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null || analyticalModel.AdjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            string zoneCategoryName = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref zoneCategoryName);
            }

            double? setbackFlowRateFactor = null;
            index = Params.IndexOfInputParam("setbackFlowRateFactor_");
            if (index != -1)
            {
                double setbackFlowRateFactor_Input = double.NaN;
                if (dataAccess.GetData(index, ref setbackFlowRateFactor_Input))
                {
                    if (PartFData.IsValidSetbackFlowRateFactor(setbackFlowRateFactor_Input))
                    {
                        setbackFlowRateFactor = setbackFlowRateFactor_Input;
                    }
                    else
                    {
                        //Reported rather than silently substituted: a factor above 1 would give a setback
                        //rate above the continuous design rate, and NaN would poison every rate.
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("setbackFlowRateFactor_ must be greater than 0 and no greater than 1. '{0}' was ignored and the default {1} used instead.", setbackFlowRateFactor_Input, PartFData.DefaultSetbackFlowRateFactor));
                    }
                }
            }

            AnalyticalModel analyticalModel_Result = analyticalModel.AddTransferAirDoorsByPartF(zoneCategoryName, setbackFlowRateFactor, out List<Aperture> doors_Created, out List<string> notes, out List<string> refusals);
            if (analyticalModel_Result == null)
            {
                foreach (string refusal in refusals)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, refusal);
                }

                return;
            }

            foreach (string note in notes)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, note);
            }

            foreach (string refusal in refusals)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, refusal);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel_Result);
            }

            index = Params.IndexOfOutputParam("doors");
            if (index != -1)
            {
                dataAccess.SetDataList(index, doors_Created?.ConvertAll(x => new GooAperture(x)));
            }

            index = Params.IndexOfOutputParam("notes");
            if (index != -1)
            {
                dataAccess.SetDataList(index, notes);
            }

            index = Params.IndexOfOutputParam("unresolved");
            if (index != -1)
            {
                dataAccess.SetDataList(index, refusals);
            }
        }
    }
}
