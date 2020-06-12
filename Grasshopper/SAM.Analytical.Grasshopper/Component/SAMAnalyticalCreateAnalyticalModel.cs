using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAnalyticalModel : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2a22123a-84c5-48dc-b207-c63b430cbca0");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAnalyticalModel()
          : base("SAMAnalytical.CreateAnalyticalModelByAdjacencyCluster", "SAMAnalytical.CreateAnalyticalModelByAdjacencyCluster",
              "Create Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddTextParameter("_name_", "_name_", "Analytical Model Name", GH_ParamAccess.item, string.Empty);
            inputParamManager.AddTextParameter("_description_", "_description_", "SAM Description", GH_ParamAccess.item, string.Empty);
            inputParamManager.AddParameter(new GooLocationParam(), "_location", "_location", "SAM Location", GH_ParamAccess.item);

            index = inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "adjacencyCluster_", "adjacencyCluster_", "SAM Adjacency Cluster", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooPanelParam(), "panels_", "panels_", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalModelParam(), "AnalyticalModel", "AnalyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(0, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string description = null;
            if (!dataAccess.GetData(1, ref description) || description == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.Location location = null;
            if (!dataAccess.GetData(2, ref location) || location == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            dataAccess.GetData(3, ref adjacencyCluster);
            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();
            else
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels = new List<Panel>();
            dataAccess.GetDataList(4, panels);
            if(panels != null && panels.Count > 0)
            {
                foreach (Panel panel in panels)
                    adjacencyCluster.AddObject(panel);
            }

            dataAccess.SetData(0, new GooAnalyticalModel(new AnalyticalModel(name, description, location, null, adjacencyCluster)));
        }
    }
}