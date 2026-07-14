// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetInternalMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d3f20b86-87c4-4214-ba54-37e6effb23f7");

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
        public SAMAnalyticalGetInternalMaterials()
          : base("SAMAnalytical.GetInternalMaterials", "SAMAnalytical.GetInternalMaterials",
              "Gets Internal Materials from SAM AdjacencyCluster",
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

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "_materialLibrary", NickName = "_materialLibrary", Description = "SAM MaterialLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "Materials", NickName = "Materials", Description = "SAM Materials", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_adjacencyCluster");
            AdjacencyCluster adjacencyCluster = null;
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = new List<Space>();
            index = Params.IndexOfInputParam("_spaces_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            if (spaces == null || spaces.Count == 0)
                spaces = adjacencyCluster.GetSpaces();

            MaterialLibrary materialLibrary = null;
            index = Params.IndexOfInputParam("_materialLibrary");
            if (index != -1)
            {
                dataAccess.GetData(index, ref materialLibrary);
            }
            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            if (spaces != null && spaces.Count != 0)
            {
                List<Space> spaces_Temp = new List<Space>();

                DataTree<GooMaterial> dataTree_Materials = new DataTree<GooMaterial>();
                DataTree<GooPanel> dataTree_Panels = new DataTree<GooPanel>();
                DataTree<double> dataTree_Areas = new DataTree<double>();

                int count = 0;
                foreach (Space space in spaces)
                {
                    spaces_Temp.Add(space);

                    GH_Path path = new GH_Path(count);

                    Dictionary<Panel, IMaterial> dictionary = Analytical.Query.InternalMaterialDictionary(space, adjacencyCluster, materialLibrary);
                    if (dictionary != null)
                    {
                        foreach (KeyValuePair<Panel, IMaterial> keyValuePair in dictionary)
                        {
                            dataTree_Materials.Add(new GooMaterial(keyValuePair.Value), path);
                            dataTree_Panels.Add(new GooPanel(keyValuePair.Key), path);
                            dataTree_Areas.Add(keyValuePair.Key.GetArea(), path);
                        }
                    }
                    count++;
                }

                index = Params.IndexOfOutputParam("Spaces");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, spaces_Temp.ConvertAll(x => new GooSpace(x)));
                }

                index = Params.IndexOfOutputParam("Materials");
                if (index != -1)
                {
                    dataAccess.SetDataTree(index, dataTree_Materials);
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
