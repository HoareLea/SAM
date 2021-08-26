using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryMergeOverlaps : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2b27fa9e-6ceb-45e5-a99c-b6d057173d5c");

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
        public SAMGeometryMergeOverlaps()
          : base("SAMGeometry.MergeOverlaps", "SAMGeometry.MergeOverlaps",
              "Merge Face3Ds Overlaps",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_face3Ds", "_face3Ds", "SAM Geometry Face3Ds", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "face3Ds", "face3Ds", "SAM Geometry Face3Ds", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp))
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            if (face3Ds != null && face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            if (face3Ds.Count > 1)
            {
                Plane plane = face3Ds[0].GetPlane();

                List<Planar.Face2D> face2Ds = face3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));

                Planar.Modify.MergeOverlaps(face2Ds);

                face3Ds = face2Ds.ConvertAll(x => plane.Convert(x));
            }

            dataAccess.SetDataList(0, face3Ds.ConvertAll(x => new GooSAMGeometry(x)));
            dataAccess.SetData(1, true);
        }
    }
}