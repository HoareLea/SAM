using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelSpacing : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32fcd1a9-7926-45de-bd27-3dde667ba0d0");

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
        public SAMAnalyticalPanelSpacing()
          : base("SAMAnalytical.PanelSpacing", "SAMAnalytical.PanelSpacing",
              "Calculates Spacing between Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_SAMPanels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("max", "max", "Maximal distance to be checked", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
            inputParamManager.AddNumberParameter("min", "min", "Minimal distance to be checked", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double max = Core.Tolerance.MacroDistance;
            if (!dataAccess.GetData(1, ref max))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double min = Core.Tolerance.Distance;
            if (!dataAccess.GetData(2, ref min))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Point3D> point3Ds = Analytical.Query.SpacingPoint3Ds(panels, max, min);

            dataAccess.SetDataList(0, point3Ds.ToList().ConvertAll(x => x.ToRhino()));
        }
    }
}