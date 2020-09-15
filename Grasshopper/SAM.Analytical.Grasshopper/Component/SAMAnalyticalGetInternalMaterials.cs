﻿using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetInternalMaterials : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d3f20b86-87c4-4214-ba54-37e6effb23f7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetInternalMaterials()
          : base("SAMAnalytical.GetInternalMaterials", "SAMAnalytical.GetInternalMaterials",
              "Gets Internal Materials from SAM AdjacencyCluster",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);

            GooSpaceParam gooSpaceParam = new GooSpaceParam();
            gooSpaceParam.Optional = true;

            inputParamManager.AddParameter(gooSpaceParam, "_spaces", "_spaces", "SAM Analytical Spaces", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary", "_materialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
        }
        
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSpaceParam(), "Spaces", "Spaces", "SAM Spaces", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooMaterialParam(), "Materials", "Materials", "SAM Materials", GH_ParamAccess.tree);
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "Panels", GH_ParamAccess.tree);
            outputParamManager.AddNumberParameter("Areas", "Areas", "Areas", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetDataList(0, null);
            dataAccess.SetDataList(1, null);
            dataAccess.SetDataList(2, null);
            dataAccess.SetDataList(3, null);

            AdjacencyCluster adjacencyCluster = null;
            if(!dataAccess.GetData(0, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = new List<Space>();
            dataAccess.GetDataList(1, spaces);

            if (spaces == null || spaces.Count == 0)
                spaces = adjacencyCluster.GetSpaces();

            MaterialLibrary materialLibrary = null;
            if (!dataAccess.GetData(2, ref materialLibrary) || materialLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(spaces != null && spaces.Count != 0)
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

                dataAccess.SetDataList(0, spaces.ConvertAll(x => new GooSpace(x)));
                dataAccess.SetDataTree(1, dataTree_Materials);
                dataAccess.SetDataTree(2, dataTree_Panels);
                dataAccess.SetDataTree(3, dataTree_Areas);
            }
        }
    }
}