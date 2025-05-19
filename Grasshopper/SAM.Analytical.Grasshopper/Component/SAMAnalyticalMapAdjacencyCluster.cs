using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMapAdjacencyCluster : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("78bb8def-b986-4068-8c78-e91d262862ec");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMapAdjacencyCluster()
          : base("SAMAnalytical.MapAdjacencyCluster", "SAMAnalytical.MapAdjacencyCluster",
              "Map AdjacencyCLuster",
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

                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam()
                {
                    Name = "_adjacencyCluster_Source",
                    NickName = "_adjacencyCluster_Source",
                    Description = "Source SAM Analytical AdjacencyCluster",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam()
                {
                    Name = "_adjacencyCluster_Destination",
                    NickName = "_adjacencyCluster_Destination",
                    Description = "Source SAM Analytical AdjacencyCluster",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_adjacencyCluster_Source");
            AdjacencyCluster adjacencyCluster_Source = null;
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster_Source) || adjacencyCluster_Source == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_adjacencyCluster_Destination");
            AdjacencyCluster adjacencyCluster_Destination = null;
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster_Destination) || adjacencyCluster_Destination == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            adjacencyCluster_Destination = new AdjacencyCluster(adjacencyCluster_Destination);
            Analytical.Modify.Map(adjacencyCluster_Source, adjacencyCluster_Destination);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster_Destination));
        }
    }
}