using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByPoints : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e61f2f2e-f655-430a-9dfd-507929edef58");

        /// <summary>
        /// ` Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByPoints()
          : base("SAMAnalytical.SnapByPoints", "SAMAnalytical.SnapByPoints",
              "Snap Panels to Points",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            inputParamManager.AddParameter(new GooPanelParam(), "_Panel", "_Panel", "SAM Analytical Panel", GH_ParamAccess.item);
            index = inputParamManager.AddParameter(new Geometry.Grasshopper.GooSAMGeometryParam(), "_points", "_points", "List of Points", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Max Distance to snap points default 1m", GH_ParamAccess.item, 1);
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

            List<Geometry.ISAMGeometry> geometries = new List<Geometry.ISAMGeometry>();

            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList(1, geometries) || geometries == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;
            if (!dataAccess.GetData(2, ref maxDistance) || double.IsNaN(maxDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.Point3D> point3Ds = geometries?.Cast<Geometry.Spatial.Point3D>()?.ToList();
            Geometry.Spatial.Modify.RemoveAlmostSimilar(point3Ds);

            Panel result = new Panel(panel);
            result.Snap(point3Ds, maxDistance);

            dataAccess.SetData(0, new GooPanel(result));
        }
    }
}