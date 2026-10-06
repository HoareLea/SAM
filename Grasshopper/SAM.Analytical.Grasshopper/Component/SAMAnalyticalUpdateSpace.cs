// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f700cfb-0344-442f-b882-b16ffa499bc3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateSpaces()
          : base("SAMAnalytical.UpdateSpaces", "SAMAnalytical.UpdateSpaces",
              "Update Spaces in SAM Adjacency Cluster or Analytical Model \n*If Space with new location is connected \n exisitng space will be replace by new one",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Model or Adjacency Cluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Model or Adjacency Cluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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

            List<Space> spaces = new List<Space>();
            index = Params.IndexOfInputParam("_spaces");
            if (index == -1 || !dataAccess.GetDataList(index, spaces))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            if (adjacencyCluster == null && sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            // Single call instead of N per-space calls — the bulk overload caches existing-space shells
            // once instead of rebuilding them on every iteration. On a 1000-space model this collapses
            // a ~35 s loop to a few seconds.
            adjacencyCluster.UpdateSpaces(spaces);

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
            {
                if (sAMObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = ((AnalyticalModel)sAMObject);
                    analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(index, new GooJSAMObject<SAMObject>(analyticalModel));
                }
                else
                {
                    dataAccess.SetData(index, new GooJSAMObject<SAMObject>(adjacencyCluster));
                }
            }
        }
    }
}
