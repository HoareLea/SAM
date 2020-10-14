using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalExtendPanel : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e997e962-7b26-45f0-8f6a-c46044c393cf");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalExtendPanel()
          : base("SAMAnalytical.ExtendPanel", "SAMAnalytical.ExtendPanel",
              "Extend SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_tolerance_", "_tolerance", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panel", "Panel", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
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

            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(2, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

                Panel result = Analytical.Query.Extend(panel, panels, tolerance);

            dataAccess.SetData(0, result);
        }
    }
}