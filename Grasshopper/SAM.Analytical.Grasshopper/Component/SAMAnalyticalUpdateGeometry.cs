using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateGeometry : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8aab6290-b5bc-4052-ad5e-21ff4254946e");

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
        public SAMAnalyticalUpdateGeometry()
          : base("SAMAnalytical.UpdateGeometry", "SAMAnalytical.UpdateGeometry",
              "Update Geometry",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_geometry", "_geometry", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_copyApertures_", "_copyApertures_", "Copy apertures if possible", GH_ParamAccess.item, true);

            inputParamManager.AddBooleanParameter("simplify_", "simplify_", "Simplify", GH_ParamAccess.item, true);
            inputParamManager.AddNumberParameter("minArea_", "minArea_", "Minimal Acceptable area of Aperture", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
            inputParamManager.AddNumberParameter("tolerance_", "tolerance_", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panel", "Panel", "SAM Analytical Panel", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
                return;

            object @object = null;
            if (!dataAccess.GetData(1, ref @object))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool simplify = true;
            if (!dataAccess.GetData(3, ref simplify))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;
            if (!Query.TryConvertToPanelGeometries(@object, out geometry3Ds, simplify))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double minArea = double.NaN;
            if (!dataAccess.GetData(4, ref minArea))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = double.NaN;
            if (!dataAccess.GetData(5, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool copyApertures = true;
            if (!dataAccess.GetData(2, ref copyApertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = Create.Panels(geometry3Ds, panel.PanelType, panel.Construction, minArea, tolerance);
            if (panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid geometry for panel");
                return;
            }

            panels[0] = new Panel(panel.Guid, panel, panels[0].GetFace3D(), null, true, minArea);
            if (!copyApertures)
                panels[0].RemoveApertures();

            if (panels.Count > 1)
            {
                for (int i = 1; i < panels.Count; i++)
                {
                    panels[i] = new Panel(panels[i].Guid, panel, panels[i].GetFace3D(), null, true, minArea);
                    if (!copyApertures)
                        panels[i].RemoveApertures();
                }
            }

            dataAccess.SetDataList(0, panels.ConvertAll(x => new GooPanel(x)));
        }
    }
}