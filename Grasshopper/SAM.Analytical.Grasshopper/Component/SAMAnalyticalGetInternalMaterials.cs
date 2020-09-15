using Grasshopper.Kernel;
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
          : base("SAMAnalytical.GetInternalConstructionLayers", "SAMAnalytical.GetInternalConstructionLayers",
              "Gets Internal ConstructionLayers from SAM AdjacencyCluster",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary", "_materialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialParam(), "Materials", "Materials", "SAM Materials", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "Panels", GH_ParamAccess.list);
            outputParamManager.AddNumberParameter("Areas", "Areas", "Areas", GH_ParamAccess.list);
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

            AdjacencyCluster adjacencyCluster = null;
            if(!dataAccess.GetData(0, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space = null;
            if (!dataAccess.GetData(1, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            MaterialLibrary materialLibrary = null;
            if (!dataAccess.GetData(2, ref materialLibrary) || materialLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Dictionary<Panel, IMaterial> dictionary = Analytical.Query.InternalMaterialDictionary(space, adjacencyCluster, materialLibrary);
            if(dictionary != null)
            {
                List<double> areas = new List<double>();
                List<Panel> panels = new List<Panel>();
                List<IMaterial> materials = new List<IMaterial>();

                foreach (KeyValuePair<Panel, IMaterial> keyValuePair in dictionary)
                {
                    materials.Add(keyValuePair.Value);
                    panels.Add(keyValuePair.Key);
                    areas.Add(keyValuePair.Key.GetArea());
                    
                }

                dataAccess.SetDataList(0, materials?.ConvertAll(x => new GooMaterial(x)));
                dataAccess.SetDataList(1, panels?.ConvertAll(x => new GooPanel(x)));
                dataAccess.SetDataList(2, areas);
            }
        }
    }
}