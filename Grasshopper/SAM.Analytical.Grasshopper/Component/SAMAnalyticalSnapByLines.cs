using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByLines : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32682bee-68b4-4de0-ab72-6b01233a9764");

        /// <summary>
        /// ` Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByLines()
          : base("SAMAnalytical.SnapByLines", "SAMAnalytical.SnapByLines",
              "Snap Panels to Lines",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_Panel", "_Panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddLineParameter("_lines", "_lines", "Lines", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Max Distance", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "_Panel", "_Panel", "SAM Analytical Panel", GH_ParamAccess.item);
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

            List<Line> lines = new List<Line>();
            if (!dataAccess.GetDataList(1, lines) || lines == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY;
            List<Geometry.Spatial.Plane> planes = new List<Geometry.Spatial.Plane>();

            foreach (Segment3D segment3D in lines.ConvertAll(x => x.ToSAM()))
            {
                Geometry.Spatial.Plane plane_Segment3D = new Geometry.Spatial.Plane(segment3D[0], segment3D[1], (Point3D)segment3D[0].GetMoved(plane.Normal));
                if (plane_Segment3D != null)
                    planes.Add(plane_Segment3D);
            }

            double maxDistance = double.NaN;
            if (!dataAccess.GetData(2, ref maxDistance) || double.IsNaN(maxDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel result = new Panel(panel);
            result.Snap(planes, maxDistance);

            dataAccess.SetData(0, new GooPanel(result));
        }
    }
}