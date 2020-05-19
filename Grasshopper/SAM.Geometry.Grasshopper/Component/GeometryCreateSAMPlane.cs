using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryCreateSAMPlane : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d146aa94-8faa-43f0-a55b-d9472cde3cce");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometryCreateSAMPlane()
          : base("Geometry.CreateSAMPlane", "Geometry.CreateSAMPlane",
              "Creates SAM Plane by points",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_points", "Points", "snapping Rhino or SAM Points", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("tolerance_", "tolerance", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Plane", "Plane", "Plane", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Normal", "Normal", "Normal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(1, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Spatial.Point3D point3D = gHObjectWrapper.Value as Spatial.Point3D;
                if (point3D != null)
                {
                    point3Ds.Add(point3D);
                    continue;
                }

                GH_Point gHPoint = gHObjectWrapper.Value as GH_Point;
                if (gHPoint == null)
                    continue;

                point3Ds.Add(Convert.ToSAM(gHPoint));
            }

            Spatial.Plane plane = Spatial.Create.Plane(point3Ds, tolerance);

            dataAccess.SetData(0, new GooSAMGeometry(plane));
            dataAccess.SetData(1, new GooSAMGeometry(plane.Normal));

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}