using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySAMGeometry3D : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f3870c7c-dd77-4022-a908-e96ac4a0bc9b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySAMGeometry3D()
          : base("SAMGeometry.SAMGeometry3D", "SAMGeometry.SAMGeometry3D",
              "Convert SAM geometry 2D to SAM geometry 3D",
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

            IBoundable3D boundable3D = null;

            if (objectWrapper.Value is GooSAMGeometry)
            {
                ISAMGeometry sAMGeometry = ((GooSAMGeometry)objectWrapper.Value).Value;
                if(sAMGeometry is ISAMGeometry2D)
                {
                    dataAccess.SetData(0, sAMGeometry);
                    return;
                }

                boundable3D = sAMGeometry as IBoundable3D;
            }
            else if (objectWrapper.Value is IBoundable3D)
            {
                boundable3D = objectWrapper.Value as dynamic;
            }

            if (boundable3D == null)
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

            Plane plane = null;

            bool ownPlane = gHBoolean.Value;
            if (ownPlane && boundable3D is IPlanar3D)
                plane = (boundable3D as IPlanar3D).GetPlane();

            if (plane == null)
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

            Planar.ISAMGeometry2D geometry2D = plane.Convert(boundable3D);
            if (geometry2D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
                return;
            }

            dataAccess.SetData(0, geometry2D);
        }
    }
}