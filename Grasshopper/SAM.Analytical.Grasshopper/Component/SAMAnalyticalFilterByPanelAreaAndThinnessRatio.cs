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
        public override Guid ComponentGuid => new Guid("455ec08e-285d-4fcd-be03-52c99c509e74");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByPanelAreaAndThinnessRatio()
          : base("SAMAnalytical.FilterByPanelAreaAndThinnessRatio", "SAMAnalytical.FilterByPanelAreaAndThinnessRatio",
              "Filters Analytcial Model/AdjacencyCluster by Panel Area And ThinnessRatio. To filter only by Areau use ThinnessRatio = 1",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_analytical", "_analytical", "SAM Analytical AdjacencyCluster or AnalyticalModel", GH_ParamAccess.item);

            int index = -1;
            index = inputParamManager.AddNumberParameter("_minArea_", "_minArea_", "Minimal Area", GH_ParamAccess.item, 0.003);
            index = inputParamManager.AddNumberParameter("_minThinnessRatio_", "_minThinnessRatio_", "Minimal Thinness Ratio", GH_ParamAccess.item, 0.003);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Analytical", "Analytical", "SAM Analytical AdjacencyCluster or AnalyticalModel", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooPanelParam(), "In", "In", "SAM Analytical Panels left in SAMAnlayticalCluster", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Out", "Out", "SAM Analytical Panels removed from SAMAnlayticalCluster", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Core.SAMObject sAMObject = null;
            if(!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
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

            AdjacencyCluster adjacencyCluster = null;
            List<Panel> panels = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                panels = adjacencyCluster?.GetPanels();
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                panels = adjacencyCluster?.GetPanels();
            }
            else if(sAMObject is Panel)
            {
                panels = new List<Panel>() { (Panel)sAMObject };
            }

            if (panels == null || panels.Count == 0)
            {
                dataAccess.SetData(0, sAMObject);
                dataAccess.SetDataList(1, null);
                dataAccess.SetDataList(2, null);
                return;
            }

            List<Panel> panels_In = new List<Panel>();
            List<Panel> panels_Out = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                double area = panel.GetArea();
                if (area > minArea)
                {
                    panels_In.Add(Create.Panel(panel));
                    continue;
                }


                double thinnessRatio = panel.GetThinnessRatio();
                if (thinnessRatio > minThinnessRatio)
                {
                    panels_In.Add(Create.Panel(panel));
                    continue;
                }

                adjacencyCluster?.RemoveObject<Panel>(panel.Guid);
                panels_Out.Add(Create.Panel(panel));
            }

            if (sAMObject is AdjacencyCluster)
            {
                dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster));
            }
            else if(sAMObject is AnalyticalModel)
            {
                dataAccess.SetData(0, new GooAnalyticalModel(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster)));
            }
            else
            {
                dataAccess.SetData(0, sAMObject);
            }

            dataAccess.SetDataList(1, panels_In?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(2, panels_Out?.ConvertAll(x => new GooPanel(x)));
        }
    }
}