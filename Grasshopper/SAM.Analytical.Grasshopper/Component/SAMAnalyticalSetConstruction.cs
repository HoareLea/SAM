using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstruction : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5442e313-bf9f-4292-a740-232f01c182a3");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetConstruction()
          : base("SAMAnalytical.SetConstruction", "SAMAnalytical.SetConstruction",
              "Set Construction",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooConstructionParam(), "_construction", "_construction", "SAM Analytical Panels", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
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
            if (!dataAccess.GetData(0, ref adjacencyCluster))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(1, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Construction construction = null;
            if (!dataAccess.GetData(2, ref construction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Panel> panels_Result = new List<Panel>();

            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Panel panel_AdjacencyCluster = adjacencyCluster_Result.GetObject<Panel>(panel.Guid);
                if (panel_AdjacencyCluster == null)
                    continue;

                panel_AdjacencyCluster = new Panel(panel_AdjacencyCluster, construction);
                if (panel_AdjacencyCluster == null)
                    continue;

                if (adjacencyCluster_Result.AddObject(panel_AdjacencyCluster))
                    panels_Result.Add(panel_AdjacencyCluster);
            }

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster_Result));
            dataAccess.SetDataList(1, panels_Result.ConvertAll(x => new GooPanel(x)));
        }
    }
}