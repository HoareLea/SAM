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
    public class SAMAnalyticalPlaneIntersection : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8245dff3-2273-4f02-9f0c-53cceb1030af");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Intersect Panels with Plance and return SAM.Geometry Edge3Ds
        /// </summary>
        public SAMAnalyticalPlaneIntersection()
          : base("SAMAnalytical.PlaneIntersection", "SAMAnalytical.PlaneIntersection",
              "Gets SAM.Geometry Edge3Ds from Analytical Object that intersect with Planne by Elevation",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Core.SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object ie.Panel, Face3d", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_elevation", "_elevation", "Elevation", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Edge3Ds", "Edge3Ds", "SAM.Geometry Edge3Ds", GH_ParamAccess.list);
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

            Core.SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || !(sAMObject is Panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = 0;
            if (!dataAccess.GetData(1, ref elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel panel = (Panel)sAMObject;

            IEnumerable<ICurve3D> edge3Ds = panel.GetEdge3Ds(elevation);
            if (edge3Ds != null && edge3Ds.Count() > 0)
                dataAccess.SetDataList(0, edge3Ds.ToList().ConvertAll(x => new GooSAMGeometry(x)));
        }
    }
}