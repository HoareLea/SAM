using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByPanelAreaAndThinnessRatio : GH_SAMComponent
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
        public SAMAnalyticalFilterByPanelAreaAndThinnessRatio()
          : base("SAMAnalytical.FilterByPanelAreaAndThinnessRatio", "SAMAnalytical.FilterByPanelAreaAndThinnessRatio",
              "Filters AdjacencyCluster by Panel Area And ThinnessRatio",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);

            int index = -1;
            index = inputParamManager.AddNumberParameter("_minArea_", "_minArea_", "Minimal Area", GH_ParamAccess.item, 0.003);
            index = inputParamManager.AddNumberParameter("_minThinnessRatio_", "_minThinnessRatio_", "Minimal Thinness Ratio", GH_ParamAccess.item, 0.003);
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

            double minArea = 0.003;
            if (!dataAccess.GetData(1, ref minArea))
                minArea = 0.003;

            double minThinnessRatio = 0.003;
            if (!dataAccess.GetData(2, ref minThinnessRatio))
                minThinnessRatio = 0.003;

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                foreach(Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    double area = panel.GetArea();
                    if (area > minArea)
                        continue;

                    double thinnessRatio = panel.GetThinnessRatio();
                    if (thinnessRatio > minThinnessRatio)
                        continue;

                    adjacencyCluster.RemoveObject<Panel>(panel.Guid);
                }
            }

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}