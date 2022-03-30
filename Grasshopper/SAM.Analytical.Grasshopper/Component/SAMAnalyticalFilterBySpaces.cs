using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterBySpaces : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c03a5e71-9800-4f7a-8098-409fa723c540");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterBySpaces()
          : base("SAMAnalytical.FilterBySpaces", "SAMAnalytical.FilterBySpaces",
              "Filters AdjacencyCluster by Spaces \n*This is used to create AdjCluster from few selected sapces",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSpaceParam(), "Spaces", "Spaces", "SAM Geometry Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AdjacencyCluster adjacencyCluster = null;
            if(!dataAccess.GetData(0, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = new List<Space>();
            if (!dataAccess.GetDataList(1, spaces) || spaces == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster.Filter(spaces)));
        }
    }
}