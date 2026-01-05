// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByApertureConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new("3f0618c7-7a51-4e61-88c9-60da22ccb245");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateCaseByApertureConstruction()
          : base("SAMAnalytical.CreateCaseByApertureConstruction", "SAMAnalytical.CreateCaseByApertureConstruction",
              "AAA",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new() { Name = "_baseAModel", NickName = "_baseAModel", Description = "Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooApertureParam gooApertureParam = new() { Name = "_apertures_", NickName = "_apertures_", Description = "SAM Apertures", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                GooApertureConstructionParam gooApertureConstructionParam = new() { Name = "_apertureConstruction_", NickName = "_apertureConstruction_", Description = "SAM ApertureConstruction", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooApertureConstructionParam, ParamVisibility.Binding));

                Param_Boolean param_Boolean = new Param_Boolean
                {
                    Name = "_concatenate_",
                    NickName = "_concatenate_",
                    Description = "concatenate",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new() { Name = "CaseAModel", NickName = "CaseAModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                Param_String param_String = new() { Name = "CaseDescription", NickName = "CaseDescription", Description = "Case Description", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
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
            int index = -1;
            index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            index = Params.IndexOfInputParam("_apertureConstruction_");
            ApertureConstruction? apertureConstruction = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstruction);
            }

            int index_Concatenate = Params.IndexOfInputParam("_concatenate_");
            bool concatenate = true;
            if (index_Concatenate != -1)
            {
                dataAccess.GetData(index_Concatenate, ref concatenate);
            }

            if (!concatenate)
            {
                analyticalModel = new AnalyticalModel(analyticalModel);
                analyticalModel.RemoveValue("CaseDescription");
            }

            analyticalModel = Create.AnalyticalModel_ByApertureConstruction(analyticalModel, apertureConstruction);

            index = Params.IndexOfOutputParam("CaseDescription");
            if (index != -1)
            {

                string caseDescription = string.Empty;
                if (!Core.Query.TryGetValue(analyticalModel, "CaseDescription", out caseDescription))
                {
                    caseDescription = string.Empty;
                }

                dataAccess.SetData(index, caseDescription);
            }

            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}
