using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryTransform3DByPlane : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e225dce6-2be1-4244-ae27-232f615337d3");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryTransform3DByPlane()
          : base("SAMGeometry.Transform3DByPlane", "SAMGeometry.Transform3DByPlane",
              "Creates Transform3D By Plane (Plane To Origin Transformation)",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_plane", "_plane", "Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooTransform3DParam(), "Transform3D", "Transform3D", "SAM Geometry Transform3D", GH_ParamAccess.item);
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

            object value = objectWrapper.Value;
            if(value is IGH_Goo)
                value = (value as dynamic).Value;
            

            Plane plane = null;
            if (objectWrapper.Value is Plane)
                plane = (Plane)objectWrapper.Value;
            else if (objectWrapper.Value is GH_Plane)
                plane = ((GH_Plane)objectWrapper.Value).ToSAM();
            else if (objectWrapper.Value is Rhino.Geometry.Plane)
                plane = ((Rhino.Geometry.Plane)objectWrapper.Value).ToSAM();

            if(plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooTransform3D(Transform3D.GetPlaneToOrigin(plane)));
        }
    }
}