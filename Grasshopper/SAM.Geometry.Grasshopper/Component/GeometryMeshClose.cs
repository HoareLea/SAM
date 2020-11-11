using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryMeshClose : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e9ea89ab-6df3-4050-9a3f-134d2445d57c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometryMeshClose()
          : base("Geometry.MeshClose", "Geometry.MeshClose",
              "Close Rhino Mesh",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddMeshParameter("_mesh", "_mesh", "Rhino Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Closed", "Closed", "Closed", GH_ParamAccess.item);
            //outputParamManager.AddParameter( string, "Normal", "Normal", "Normal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Mesh mesh = null;
            if (!dataAccess.GetData(0, ref mesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Mesh mesh_Result = null;
            bool result = Query.TryClose(mesh, out mesh_Result);

            dataAccess.SetData(0, mesh_Result);
            dataAccess.SetData(1, result);
        }
    }
}