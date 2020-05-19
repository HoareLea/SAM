using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class CreateSAMTransform3DOriginToPlane : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ae182b63-cfdb-4a83-a466-9d1bc1eb42f1");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public CreateSAMTransform3DOriginToPlane()
          : base("Create.SAMTransform3DOriginToPlane", "Create.Transoform3DOriginToPlane",
              "Creates SAM Transform3D By Origin To Plane",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_plane", "_plane", "Rhino or SAM Plane", GH_ParamAccess.item);
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
            if (value is IGH_Goo)
                value = (value as dynamic).Value;

            Spatial.Plane plane = null;
            if (value is Spatial.Plane)
                plane = (Spatial.Plane)value;
            else if (value is GH_Plane)
                plane = ((GH_Plane)value).ToSAM();
            else if (value is Rhino.Geometry.Plane)
                plane = ((Rhino.Geometry.Plane)value).ToSAM();

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooTransform3D(Transform3D.GetOriginToPlane(plane)));
        }
    }
}