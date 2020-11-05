using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalJoinPanels : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8a229d7b-6cce-4a28-8be9-07dd8c17aee5");

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
        public SAMAnalyticalJoinPanels()
          : base("SAMAnalytical.JoinPanels", "SAMAnalytical.JoinPanels",
              "Join SAM Analytical Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Maximum Distance to Adjust Panels", GH_ParamAccess.item, 0.52);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Changed", "Changed", "Changed SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Removed", "Removed", "Removed SAM Analytical Panels", GH_ParamAccess.list);
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
            if (!dataAccess.GetData(2, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!run)
                return;

            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;
            if (!dataAccess.GetData(1, ref maxDistance))
                return;

            List<Panel> panels_Result = panels.ConvertAll(x => x.Clone());

            List<Guid> guids = panels_Result.Join(maxDistance);
            if (guids == null)
                return;

            List<Panel> panels_Removed = new List<Panel>();
            List<Panel> panels_Changed = new List<Panel>();

            foreach(Guid guid in guids)
            {
                Panel panel = panels_Result.Find(x => x.Guid == guid);
                if (panel == null)
                {
                    panels_Removed.Add(panels.Find(x => x.Guid == guid).Clone());
                    continue;
                }

                panels_Changed.Add(panel);
            }

            dataAccess.SetDataList(0, panels_Result.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, panels_Changed.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(2, panels_Removed.ConvertAll(x => new GooPanel(x)));
        }
    }
}