using System;
using System.Collections;

using Grasshopper.Kernel;

using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryGeometry()
          : base("SAMGeometry.Geometry", "SAMGeometry.Geometry",
              "Convert SAM geometry to Rhino geometry",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooGeometry3DParam(), "_SAMGeometry", "_SAMGeometry", "SAM Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGeometryParameter("Geometry", "Geo", "Rhino geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            IGeometry3D geometry3D = null;
            if (!dataAccess.GetData(0, ref geometry3D) || geometry3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = (geometry3D).ToGrasshopper();

            if (@object == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            else if (@object is IEnumerable)
                dataAccess.SetDataList(0, (IEnumerable)@object);
            else
                dataAccess.SetData(0, @object);
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
            get { return new Guid("111fcc37-a02e-4b5e-aaa5-0988171b6143"); }
        }
    }
}