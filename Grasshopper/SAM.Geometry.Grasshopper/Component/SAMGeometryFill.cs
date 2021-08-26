using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryFill : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4c2d4620-26c2-4b49-9b63-572dcd3ebbf5");

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
        public SAMGeometryFill()
          : base("SAMGeometry.Fill", "SAMGeometry.Fill",
              "Fill Face3D",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_face3D", "_face3D", "SAM Geometry Face3D bo be filled", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_face3Ds", "_face3Ds", "Filling SAM Geometry Face3Ds", GH_ParamAccess.list);
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
            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<Face3D> face3Ds_Temp = null;

            Face3D face3D = null;
            if (Query.TryGetSAMGeometries(objectWrapper, out face3Ds_Temp))
            {
                face3D = face3Ds_Temp.FirstOrDefault();
            }

            if (face3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            face3Ds_Temp = null;

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
            {
                if(Query.TryGetSAMGeometries(objectWrapper_Temp, out face3Ds_Temp))
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

            List<Face3D> result = face3D.Fill(face3Ds, 0.1, Core.Tolerance.Distance);

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooSAMGeometry(x)));
            dataAccess.SetData(1, true);
        }
    }
}