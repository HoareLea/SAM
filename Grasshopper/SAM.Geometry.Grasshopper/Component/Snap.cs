using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper.Properties;

namespace SAM.Geometry.Grasshopper
{
    public class Snap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public Snap()
          : base("Snap", "Snp",
              "Snap Geometry",
              "SAM", "Geometry")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Geometry", "Geo", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("Points", "Points", "Points", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("maxDistance", "mDis", "Max Distance to snap points", GH_ParamAccess.item, double.NaN);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Geometry", "Geo", "SAM Geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
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


            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_Number gHNumber = objectWrapper.Value as GH_Number;
            if (gHNumber == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;
            if(obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Spatial.IGeometry3D geometry3D = null;

            if (obj is IGH_GeometricGoo)
                geometry3D = Convert.ToSAM((IGH_GeometricGoo)obj);
            else if (obj is Spatial.IGeometry3D)
                geometry3D = (Spatial.IGeometry3D)obj;

            if(geometry3D is Spatial.Point3D)
            {
                geometry3D = Spatial.Point3D.Snap(point3Ds,(Spatial.Point3D)geometry3D, gHNumber.Value);
            }
            else if(geometry3D is Spatial.Segment3D)
            {
                geometry3D = Spatial.Segment3D.Snap(point3Ds, (Spatial.Segment3D)geometry3D, gHNumber.Value);
            }
            else if (geometry3D is Spatial.Polygon3D)
            {
                geometry3D = Spatial.Polygon3D.Snap(point3Ds, (Spatial.Polygon3D)geometry3D, gHNumber.Value);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, geometry3D);

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
                return Resources.HL_Logo24;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1165eb14-9f8a-460e-a3e7-e9b3d7f08ddc"); }
        }
    }
}