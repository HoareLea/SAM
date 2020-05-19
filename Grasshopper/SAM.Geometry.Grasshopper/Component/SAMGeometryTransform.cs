using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryTransform : GH_SAMComponent
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryTransform()
          : base("SAMGeometry.Transform", "SAMGeometry.Transform",
              "Transforms SAM Geometry",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMGeometryParam(), "_SAMGeometry", "_SAMGeometry", "SAM Geometry", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooTransform3DParam(), "_transform3D", "_transform3D", "SAM Transform 3D", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "SAMGeometry", "SAMGeometry", "SAM Geometry", GH_ParamAccess.item);
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

            Spatial.Transform3D transform3D = null;
            if (!dataAccess.GetData(1, ref transform3D) || transform3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            geometry = Spatial.Query.Transform(geometry as dynamic, transform3D);
            if (geometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry could not be transformed");
                return;
            }

            dataAccess.SetData(0, new GooSAMGeometry(geometry));
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
            get { return new Guid("ca12bdd8-d050-4bf1-9c37-963cfee655f3"); }
        }
    }
}