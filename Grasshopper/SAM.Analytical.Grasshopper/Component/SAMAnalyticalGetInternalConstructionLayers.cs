using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetInternalConstructionLayers : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3beff062-1428-4eb9-bd97-9077f082db5b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetInternalConstructionLayers()
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionLayerParam(), "ConstructionLayers", "ConstructionLayers", "SAM Analytical ConstructionLayers", GH_ParamAccess.list);
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

            Dictionary<Panel, ConstructionLayer> dictionary = Analytical.Query.InternalConstructionLayerDictionary(space, adjacencyCluster);
            if(dictionary != null)
            {
                List<double> areas = new List<double>();
                List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();

                foreach(KeyValuePair<Panel, ConstructionLayer> keyValuePair in dictionary)
                {
                    areas.Add(keyValuePair.Key.GetArea());
                    constructionLayers.Add(keyValuePair.Value);
                }

                dataAccess.SetDataList(0, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
                dataAccess.SetDataList(1, areas);
            }
        }
    }
}