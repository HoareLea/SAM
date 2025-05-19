using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilter : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("dea0cff4-87ca-428b-bf9a-c25d39742180");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilter()
          : base("SAMAnalytical.Filter", "SAMAnalytical.Filter",
              "Filter Adjacency Cluster",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical Adjacency Cluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSpaceParam(), "_spaces", "_spaces", "SAM Analytical Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical Adjacency Cluster", GH_ParamAccess.item);
            outputParamManager.AddGenericParameter("Objects", "Objects", "SAM Objects", GH_ParamAccess.list);
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

            AdjacencyCluster adjacencyCluster_New = new AdjacencyCluster();

            List<object> objects = new List<object>();
            foreach(Space space in spaces)
            {
                adjacencyCluster_New.AddObject(space);

                List<IJSAMObject> relatedObjects = adjacencyCluster.GetRelatedObjects(space);
                if (relatedObjects == null || relatedObjects.Count == 0)
                    continue;

                foreach(IJSAMObject relatedObject in relatedObjects)
                {
                    if (!adjacencyCluster_New.AddObject(relatedObject))
                        continue;

                    objects.Add(relatedObject);
                    adjacencyCluster_New.AddRelation(space, relatedObject);
                }
            }

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster_New));
            dataAccess.SetDataList(1, objects);
        }
    }
}