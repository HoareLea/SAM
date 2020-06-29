using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateNormals : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5a2b947c-10cd-42cb-8f73-1764138236aa");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateNormals()
          : base("SAMAnalytical.UpdateNormals", "SAMAnalytical.UpdateNormals",
              "Add Space to SAM Analytical Model",
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
            inputParamManager.AddParameter(gooSpaceParam, "space_", "space_", "SAM Analytical Space", GH_ParamAccess.item);
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

            Space space = null;
            dataAccess.GetData(1, ref space);

            AdjacencyCluster adjacencyCluster_New = null;
            if(space == null)
            {
                adjacencyCluster_New = adjacencyCluster.UpdateNormals();
            }
            else
            {
                adjacencyCluster_New = adjacencyCluster.Filter(new Space[] { space });
                List<Panel> panels = adjacencyCluster_New.UpdateNormals(space);
                if (panels != null)
                    panels.ForEach(x => adjacencyCluster_New.AddObject(x));
            }

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster_New));
        }
    }
}