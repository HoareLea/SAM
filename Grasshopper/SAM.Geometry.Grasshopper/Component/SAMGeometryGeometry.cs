using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryGeometry : GH_SAMComponent
    {
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("111fcc37-a02e-4b5e-aaa5-0988171b6143");

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
            inputParamManager.AddParameter(new GooSAMGeometryParam(), "_SAMGeometry", "_SAMGeometry", "SAM Geometry", GH_ParamAccess.item);
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
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            ISAMGeometry geometry = null;
            if (!dataAccess.GetData(0, ref geometry) || geometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = geometry.ToGrasshopper();

            if (@object == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            else if (@object is IEnumerable)
                dataAccess.SetDataList(0, (IEnumerable)@object);
            else
                dataAccess.SetData(0, @object);
        }
    }
}