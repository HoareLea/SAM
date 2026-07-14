// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetInternalConstructionLayers : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c5573e0e-a3ce-4f38-9992-9d8f80a924af");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetInternalConstructionLayers()
          : base("SAMAnalytical.GetInternalConstructionLayers", "SAMAnalytical.GetInternalConstructionLayers",
              "Gets Internal ConstructionLAyers from SAM AdjacencyCluster",
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "ConstructionLayers", NickName = "ConstructionLayers", Description = "SAM Analytical ConstructionLayers", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Areas", NickName = "Areas", Description = "Areas", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, null);
            }

            index = Params.IndexOfOutputParam("ConstructionLayers");
            if (index != -1)
            {
                dataAccess.SetDataList(index, null);
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, null);
            }

            index = Params.IndexOfOutputParam("Areas");
            if (index != -1)
            {
                dataAccess.SetDataList(index, null);
            }

            index = Params.IndexOfInputParam("_adjacencyCluster");
            AdjacencyCluster adjacencyCluster = null;
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = new List<Space>();
            index = Params.IndexOfInputParam("_spaces");
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            if (spaces == null || spaces.Count == 0)
                spaces = adjacencyCluster.GetSpaces();

            if (spaces != null && spaces.Count != 0)
            {
                List<Space> spaces_Temp = new List<Space>();

                DataTree<GooConstructionLayer> dataTree_ConstructionLayers = new DataTree<GooConstructionLayer>();
                DataTree<GooPanel> dataTree_Panels = new DataTree<GooPanel>();
                DataTree<double> dataTree_Areas = new DataTree<double>();

                int count = 0;
                foreach (Space space in spaces)
                {
                    spaces_Temp.Add(space);

                    GH_Path path = new GH_Path(count);

                    Dictionary<Panel, ConstructionLayer> dictionary = Analytical.Query.InternalConstructionLayerDictionary(space, adjacencyCluster);
                    if (dictionary != null)
                    {
                        foreach (KeyValuePair<Panel, ConstructionLayer> keyValuePair in dictionary)
                        {
                            dataTree_ConstructionLayers.Add(new GooConstructionLayer(keyValuePair.Value), path);
                            dataTree_Panels.Add(new GooPanel(keyValuePair.Key), path);
                            dataTree_Areas.Add(keyValuePair.Key.GetArea(), path);
                        }
                    }
                    count++;
                }

                index = Params.IndexOfOutputParam("Spaces");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, spaces.ConvertAll(x => new GooSpace(x)));
                }

                index = Params.IndexOfOutputParam("ConstructionLayers");
                if (index != -1)
                {
                    dataAccess.SetDataTree(index, dataTree_ConstructionLayers);
                }

                index = Params.IndexOfOutputParam("Panels");
                if (index != -1)
                {
                    dataAccess.SetDataTree(index, dataTree_Panels);
                }

                index = Params.IndexOfOutputParam("Areas");
                if (index != -1)
                {
                    dataAccess.SetDataTree(index, dataTree_Areas);
                }
            }
        }
    }
}
