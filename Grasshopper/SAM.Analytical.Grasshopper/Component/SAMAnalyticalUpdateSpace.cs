using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f700cfb-0344-442f-b882-b16ffa499bc3");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateSpace()
          : base("SAMAnalytical.UpdateSpace", "SAMAnalytical.UpdateSpace",
              "Update Space in SAM Adjacency Cluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Model ot Adjacency Cluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analytical", "Analytical", "SAM Analytical Model or Adjacency Cluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Space> spaces = new List<Space>();
            if (!dataAccess.GetDataList(1, spaces))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            SAMObject sAMObject = null;
            if(!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            if(adjacencyCluster == null && sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            foreach (Space space in spaces)
                adjacencyCluster.UpdateSpace(space);

            if(sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = ((AnalyticalModel)sAMObject);
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
                dataAccess.SetData(0, new GooSAMObject<SAMObject>(analyticalModel));
            }
            else
            {
                dataAccess.SetData(0, new GooSAMObject<SAMObject>(adjacencyCluster));
            }
        }
    }
}