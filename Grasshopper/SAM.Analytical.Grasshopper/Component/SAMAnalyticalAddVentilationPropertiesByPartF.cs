// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Classes;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddVentilationPropertiesByPartF : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("644bf0f8-ea02-4ea3-aa03-5b7579d7ce38");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddVentilationPropertiesByPartF()
          : base("SAMAnalytical.AddVentilationPropertiesByPartF", "SAMAnalytical.AddVentilationPropertiesByPartF",
              "Add Ventilation Properties By Part F to SAM Analytical Model",
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
                List<GH_SAMParam> result = new();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                return
                [
                    new GH_SAMParam(new GooAnalyticalModelParam { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding),
                    new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
                    new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "l/s", NickName = "l/s", Description = "l/s", Access = GH_ParamAccess.list }, ParamVisibility.Binding),
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
            if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PartFData partFData = ActiveSetting.Setting.GetValue<PartFData>(AnalyticalSettingParameter.PartFData);

            PartFCalculator partFCalculator = new(partFData)
            {
                AnalyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster),
            };

            partFCalculator.Calculate();

            analyticalModel = partFCalculator.AnalyticalModel;

            List<Space> spaces = analyticalModel.GetSpaces();

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces);
            }

            index = Params.IndexOfOutputParam("l/s");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces?.ConvertAll(partFCalculator.GetFlowRate));
            }
        }
    }
}
