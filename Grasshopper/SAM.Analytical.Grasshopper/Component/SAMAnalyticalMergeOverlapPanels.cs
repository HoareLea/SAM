using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeOverlapPanels : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f90fff5-852c-480e-bf51-bc817e15e953");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMergeOverlapPanels()
          : base("SAMAnalytical.MergeOverlapPanels", "SAMAnalytical.MergeOverlapPanels",
              "Merge SAM Analytical Panels",
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

            inputParamManager.AddNumberParameter("_offset", "_offset", "Offset", GH_ParamAccess.item, 0.5);
            inputParamManager.AddBooleanParameter("_defaultConstruction_", "_defaultConstruction_", "Set default Construtcion for Panels", GH_ParamAccess.item, false);
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
            if (!dataAccess.GetData(3, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            double offset = 0.5;
            if (!dataAccess.GetData(1, ref offset))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool setDefaultConstruction = false;
            if (!dataAccess.GetData(2, ref setDefaultConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> redundantPanels = new List<Panel>();

            panels = Query.MergeOverlapPanels(panels, offset, ref redundantPanels, setDefaultConstruction, Core.Tolerance.Distance);

            dataAccess.SetDataList(0, panels?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, redundantPanels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}