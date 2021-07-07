using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryMeshReduce : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("50f45468-60e1-4a11-9c2c-0e2b01f72952");

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
        public GeometryMeshReduce()
          : base("Geometry.MeshReduce", "Geometry.MeshReduce",
              "Reduce Rhino Mesh",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddMeshParameter("_mesh", "_mesh", "Rhino Mesh", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_distortion_", "_distortion_", "Allow Distortion towards desire count", GH_ParamAccess.item, true);
            inputParamManager.AddIntegerParameter("_count_", "_count_", "Desired Polygon Count", GH_ParamAccess.item, 44);
            inputParamManager.AddIntegerParameter("_accuracy_", "_accuracy_", "Accuracy lowest 1 to 10 highest", GH_ParamAccess.item, 10);
            inputParamManager.AddBooleanParameter("_normalizedSize_", "_normalizedSize", "Normalized Face Size", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
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
            Mesh mesh = new Mesh();
            if (!dataAccess.GetData(0, ref mesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool allowDistortion = true;
            dataAccess.GetData(1, ref allowDistortion);

            int deisredPolygonCount = 0;
            dataAccess.GetData(2, ref deisredPolygonCount);

            int accuracy = 0;
            dataAccess.GetData(3, ref accuracy);

            bool normalizedSize = true;
            dataAccess.GetData(4, ref normalizedSize);

            //Mesh result = mesh.DuplicateMesh();
            Modify.Reduce(mesh, allowDistortion, deisredPolygonCount, accuracy, normalizedSize);

            dataAccess.SetData(0, mesh);
        }
    }
}