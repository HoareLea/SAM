using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalOverlapPanels : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b48cbeeb-4f49-4d30-8bb5-0e46b42451e8");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalOverlapPanels()
          : base("SAMAnalytical.OverlapPanels", "SAMAnalytical.OverlapPanels",
              "Gets Overlaping SAM Analytical Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
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
            bool run = false;
            if (!dataAccess.GetData(1, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DataTree<GooPanel> dataTree_GooPanel = null;

            List<List<Panel>> panelsList = Analytical.Query.OverlapPanels(panels);
            int count = 0;
            if (panelsList != null)
            {
                dataTree_GooPanel = new DataTree<GooPanel>();

                foreach (List<Panel> panels_Temp in panelsList)
                {
                    GH_Path path = new GH_Path(count);
                    panels_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                    panels_Temp.ForEach(x => dataTree_GooPanel.Add(new GooPanel(x), path));
                    count++;
                }
            }
            dataAccess.SetDataTree(0, dataTree_GooPanel);
        }
    }
}