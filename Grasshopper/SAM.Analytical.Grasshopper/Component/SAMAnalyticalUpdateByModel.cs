using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateByModel : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3d09900f-8fd4-4c35-9727-ab0d910473af");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateByModel()
          : base("SAMAnalytical.UpdateByModel", "SAMAnalytical.UpdateByModel",
              "Update By Model",
              "SAM WIP", "Analytical")
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

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_oldAnalytical", NickName = "_oldAnalytical", Description = "Old Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "_zones_", NickName = "_zones_", Description = "Zones", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "zones", NickName = "zones", Description = "SAM Analytical Zones", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject_Destination = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject_Destination) || analyticalObject_Destination == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Destination = null;
            if(analyticalObject_Destination is AdjacencyCluster)
            {
                adjacencyCluster_Destination = new AdjacencyCluster((AdjacencyCluster)analyticalObject_Destination);
            }
            else if(analyticalObject_Destination is AnalyticalModel)
            {
                adjacencyCluster_Destination = ((AnalyticalModel)analyticalObject_Destination).AdjacencyCluster;
                if(adjacencyCluster_Destination != null)
                {
                    adjacencyCluster_Destination = new AdjacencyCluster(adjacencyCluster_Destination);
                }
            }

            index = Params.IndexOfInputParam("_oldAnalytical");
            IAnalyticalObject analyticalObject_Source = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject_Source) || analyticalObject_Source == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Source = null;
            if (adjacencyCluster_Source is AdjacencyCluster)
            {
                adjacencyCluster_Source = new AdjacencyCluster((AdjacencyCluster)analyticalObject_Source);
            }
            else if (analyticalObject_Destination is AnalyticalModel)
            {
                adjacencyCluster_Source = ((AnalyticalModel)analyticalObject_Source).AdjacencyCluster;
                if (adjacencyCluster_Source != null)
                {
                    adjacencyCluster_Source = new AdjacencyCluster(adjacencyCluster_Source);
                }
            }


            index = Params.IndexOfInputParam("_zones_");
            List<Zone> zones = new List<Zone>();
            if (index == -1 || !dataAccess.GetDataList(index, zones) || zones == null)
            {
                zones = adjacencyCluster_Source.GetObjects<Zone>();
            }

            zones = adjacencyCluster_Destination.CopyZones(adjacencyCluster_Source, zones?.ConvertAll(x => x.Guid));

            if (analyticalObject_Destination is AdjacencyCluster)
            {
                analyticalObject_Destination = adjacencyCluster_Destination;
            }
            else if (analyticalObject_Destination is AnalyticalModel)
            {
                analyticalObject_Destination = new AnalyticalModel((AnalyticalModel)analyticalObject_Destination, adjacencyCluster_Destination);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject_Destination);
            }

            index = Params.IndexOfOutputParam("zones");
            if (index != -1)
            {
                dataAccess.SetDataList(index, zones);
            }

        }
    }
}