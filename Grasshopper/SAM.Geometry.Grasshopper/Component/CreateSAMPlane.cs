using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper.Properties;

namespace SAM.Geometry.Grasshopper
{
    public class CreateSAMPlane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public CreateSAMPlane()
          : base("Plane.Create", "Plane.Create",
              "Creates Plane by points",
              "SAM", "Geometry")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_points", "Points", "snapping Points", GH_ParamAccess.list);
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
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            foreach(GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Spatial.Point3D point3D = gHObjectWrapper.Value as Spatial.Point3D;
                if(point3D != null)
                {
                    point3Ds.Add(point3D);
                    continue;
                }

                GH_Point gHPoint = gHObjectWrapper.Value as GH_Point;
                if (gHPoint == null)
                    continue;

                point3Ds.Add(Convert.ToSAM(gHPoint));
            }

            Spatial.Plane plane = Spatial.Create.Plane(point3Ds, true);

            dataAccess.SetData(0, new GooSAMGeometry(plane));
            dataAccess.SetData(0, new GooSAMGeometry(plane.Normal));

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Geometry;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d146aa94-8faa-43f0-a55b-d9472cde3cce"); }
        }
    }
}