// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterBySpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c03a5e71-9800-4f7a-8098-409fa723c540");

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
        public SAMAnalyticalFilterBySpaces()
          : base("SAMAnalytical.FilterBySpaces", "SAMAnalytical.FilterBySpaces",
              "Filters AdjacencyCluster by Spaces \n*This is used to create AdjCluster from few selected sapces\nEach internal panel without adjacent space will be set as Adiabatic",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Geometry Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
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

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }
            else if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }

            index = Params.IndexOfInputParam("Spaces");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces) || spaces == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (adjacencyCluster != null)
            {
                adjacencyCluster = adjacencyCluster.Filter(spaces);
                if (analyticalObject is AdjacencyCluster)
                {
                    analyticalObject = adjacencyCluster;
                }
                else if (analyticalObject is AnalyticalModel)
                {
                    analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(analyticalObject));
            }
        }
    }
}
