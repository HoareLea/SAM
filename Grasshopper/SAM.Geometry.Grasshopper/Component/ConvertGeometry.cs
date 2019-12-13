using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public class ConvertGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public ConvertGeometry()
          : base("ConvertGeometry", "Cgeo",
              "Convert Geometry",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Geometry", "geo", "Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Geometry", "geo", "Geometry", GH_ParamAccess.item);
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

            object obj = objectWrapper.Value;

            if (obj is IGeometry)
            {
                if (obj is Polygon3D)
                    obj = dataAccess.SetData(0, ((Polygon3D)obj).ToGrasshopper());
                else if (obj is Polyline3D)
                    obj = dataAccess.SetData(0, ((Polyline3D)obj).ToGrasshopper());
                else if (obj is Point3D)
                    obj = dataAccess.SetData(0, ((Point3D)obj).ToGrasshopper());
                else if (obj is Segment3D)
                    obj = dataAccess.SetData(0, ((Segment3D)obj).ToGrasshopper());
                else
                    obj = dataAccess.SetData(0, (obj as dynamic).ToGrasshopper());
            }
                

            if (obj is IGH_GeometricGoo)
            {
                if (obj is GH_Curve)
                    obj = dataAccess.SetData(0, ((GH_Curve)obj).ToSAM());
                else
                    obj = dataAccess.SetData(0, (obj as dynamic).ToSAM());
            }
                
                

            if (obj == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");

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
                return Resources.SAM_Small;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("060a9d71-9c97-4502-9e30-e9a7d45d22db"); }
        }
    }
}