using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySAMGeometry2D : GH_Component
    {
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
            genericObjectParameter.PersistentData.Append(new GH_Plane(new Rhino.Geometry.Plane(new Rhino.Geometry.Point3d(0, 0, 0), new Rhino.Geometry.Vector3d(0, 0, 1))));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("SAMGeometry2D", "SAMgeo2D", "SAM Geometry 2D", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IBoundable3D geometry3D = objectWrapper.Value as IBoundable3D;
            if (geometry3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_Boolean gHBoolean = objectWrapper.Value as GH_Boolean;
            if (gHBoolean == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Spatial.Plane plane = null;

            bool ownPlane = gHBoolean.Value;
            if (ownPlane && geometry3D is IPlanar3D)
                plane = (geometry3D as IPlanar3D).GetPlane();

            if(plane == null)
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

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Planar.ISAMGeometry2D geometry2D = plane.Convert(geometry3D);
            if (geometry2D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
                return;
            }

            dataAccess.SetData(0, geometry2D);
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
            get { return new Guid("50eb9b79-0a5f-4fb7-938d-451ff9432eee"); }
        }
    }
}