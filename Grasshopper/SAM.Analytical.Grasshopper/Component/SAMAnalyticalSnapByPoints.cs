using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;


using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByPoints : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e61f2f2e-f655-430a-9dfd-507929edef58");

        /// <summary>
        /// Provides an Icon for the component.
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
            inputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Core.SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddParameter(new Geometry.Grasshopper.GooGeometry3DParam(), "_points", "_points", "List of Points", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Max Distance to snap points default 1m", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Core.SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Core.SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || !(sAMObject is Panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.ISAMGeometry3D> geometry3Ds = new List<Geometry.Spatial.ISAMGeometry3D>();

            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList(1, geometry3Ds) || geometry3Ds == null)
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

            Panel panel = new Panel((Panel)sAMObject);
            panel.Snap(geometry3Ds.Cast<Geometry.Spatial.Point3D>(), maxDistance);

            dataAccess.SetData(0, new GooPanel(panel));
        }
    }
}