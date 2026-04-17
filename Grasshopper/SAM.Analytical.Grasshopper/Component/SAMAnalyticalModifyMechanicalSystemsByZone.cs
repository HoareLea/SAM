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
    public class SAMAnalyticalModifyMechanicalSystemsByZone : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new("630a848f-318f-48d0-8156-964aa40cdcbb");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifyMechanicalSystemsByZone()
          : base("SAMAnalytical.ModifyMechanicalSystemsByZone", "SAMAnalytical.ModifyMechanicalSystemsByZone",
              "Modify Mechanical Systems By Zone",
              "SAM", "Analytical02")
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

                GooAnalyticalModelParam gooAnalyticalModelParam = new() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooAnalyticalModelParam, ParamVisibility.Binding));

                Param_String param_String = new () { Name = "_zoneCategoryName", NickName = "_zoneCategoryName", Description = "Zone Category Name", Access = GH_ParamAccess.item};
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                Param_Boolean param_Boolean = new() { Name = "_zoneCategoryNamePrefix_", NickName = "_zoneCategoryNamePrefix_", Description = "Add zone category name prefix", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "mechanicalSystems", NickName = "mechanicalSystems", Description = "SAM MechanicalSystems", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_zoneCategoryName");
            string zoneCategoryName = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategoryName) || zoneCategoryName == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_zoneCategoryNamePrefix_");
            bool addPrefix = true;
            if (index != -1)
            {
                dataAccess.GetData(index, ref addPrefix);
            }

            List<MechanicalSystem> mechanicalSystems = null;

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster is not null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

                mechanicalSystems = adjacencyCluster.SplitSystemsByZones<VentilationSystem>(zoneCategoryName, addPrefix);

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("mechanicalSystems");
            if (index != -1)
            {
                dataAccess.SetDataList(index, mechanicalSystems?.ConvertAll(x => new GooSystem(x)));
            }
        }
    }
}
