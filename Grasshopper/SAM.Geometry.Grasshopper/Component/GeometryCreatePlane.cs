using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryCreatePlane : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d146aa94-8faa-43f0-a55b-d9472cde3cce");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometryCreatePlane()
          : base("Geometry.CreatePlane", "Geometry.CreatePlane",
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
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "plane", "plane", "SAM Geometry Plane", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "normal", "normal", "normal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrappers) || objectWrappers == null)
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

            if (!Query.TryGetSAMGeometries(objectWrappers, out List<Spatial.Point3D> point3Ds) || point3Ds == null || point3Ds.Count < 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Spatial.Plane plane = Spatial.Create.Plane(point3Ds, tolerance);

            dataAccess.SetData(0, new GooSAMGeometry(plane));
            dataAccess.SetData(1, new GooSAMGeometry(plane?.Normal));
        }
    }
}