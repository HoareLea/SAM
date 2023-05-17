using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySAMGeometry2D : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("50eb9b79-0a5f-4fb7-938d-451ff9432eee");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySAMGeometry2D()
          : base("SAMGeometry.SAMGeometry2D", "SAMGeometry.SAMGeometry2D",
              "Convert SAM geometry 3D to SAM geometry 2D",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            Param_GenericObject genericObjectParameter = null;

            inputParamManager.AddGenericParameter("_SAMGeometry3D", "_SAMGeometry3D", "SAM Geometry 3D", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_ownPlane", "_ownPlane", "Projection on own plane if possible", GH_ParamAccess.item, true);

            index = inputParamManager.AddGenericParameter("Plane", "Plane", "SAM Plane", GH_ParamAccess.item);
            genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_Plane(new global::Rhino.Geometry.Plane(new global::Rhino.Geometry.Point3d(0, 0, 0), new global::Rhino.Geometry.Vector3d(0, 0, 1))));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("sAMGeometry2D", "sAMgeo2D", "SAM Geometry 2D", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<ISAMGeometry3D> sAMGeometry3Ds) || sAMGeometry3Ds == null || sAMGeometry3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IBoundable3D> boundable3Ds = new List<IBoundable3D>();
            List<Point3D> point3Ds = new List<Point3D>();

            foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
            {
                if(sAMGeometry3D is Point3D)
                {
                    point3Ds.Add((Point3D)sAMGeometry3D);
                }
                else if(sAMGeometry3D is IBoundable3D)
                {
                    boundable3Ds.Add((IBoundable3D)sAMGeometry3D);
                }
            }

            if(boundable3Ds.Count == 0 && point3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool ownPlane = true;
            if (!dataAccess.GetData(1, ref ownPlane))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = null;

            if(!ownPlane)
            {
                if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                GH_Plane gHPlane = objectWrapper.Value as GH_Plane;
                if (gHPlane == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                plane = Convert.ToSAM(gHPlane);
            }

            if(plane == null)
            {
                plane = Plane.WorldXY;
            }

            List<ISAMGeometry2D> sAMGeometry2Ds = new List<ISAMGeometry2D>();
            foreach(Point3D point3D in point3Ds)
            {
                sAMGeometry2Ds.Add(plane.Convert(point3D));
            }

            foreach(IBoundable3D boundable3D in boundable3Ds)
            {
                Plane plane_Boundable3D = plane;
                if(ownPlane && boundable3D is IPlanar3D)
                {
                    plane_Boundable3D = (boundable3D as IPlanar3D).GetPlane();
                }

                sAMGeometry2Ds.Add(plane_Boundable3D.Convert(boundable3D));
            }

            dataAccess.SetDataList(0, sAMGeometry2Ds);
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
    }
}