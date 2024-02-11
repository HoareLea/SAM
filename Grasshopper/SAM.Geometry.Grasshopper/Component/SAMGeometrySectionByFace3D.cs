using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySectionByFace3D : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e30b211f-d161-48fc-a3f3-179a075c8cce");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySectionByFace3D()
          : base("SAMGeometry.SectionByFace3D", "SAMGeometry.SectionByFace3D",
              "Create Section ",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject;

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3D", NickName = "_face3D", Description = "SAM Geometry Face3D Objects", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));


                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset", Access = GH_ParamAccess.item};
                number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "geometries", NickName = "geometries", Description = "SAM Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "length", NickName = "length", Description = "length", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_face3D");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
            {
                if (Query.TryGetSAMGeometries(objectWrapper_Temp, out List<Face3D> face3Ds_Temp) && face3Ds_Temp != null && face3Ds_Temp.Count > 0)
                    face3Ds.AddRange(face3Ds_Temp);
            }

            if (face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double offset = 0.1;
            index = Params.IndexOfInputParam("offset_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref offset);
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<ISAMGeometry3D> sAMGeometries = new List<ISAMGeometry3D>();
            foreach(Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                Plane plane = Spatial.Create.Plane(boundingBox3D.Min.Z + offset);

                PlanarIntersectionResult planarIntersectionResult = Spatial.Create.PlanarIntersectionResult(plane, face3D, Core.Tolerance.Angle, tolerance);
                if(planarIntersectionResult == null)
                {
                    continue;
                }

                sAMGeometries.AddRange(planarIntersectionResult.GetGeometry3Ds<ISAMGeometry3D>());
            }

            index = Params.IndexOfOutputParam("geometries");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sAMGeometries?.ConvertAll(x => new GooSAMGeometry(x)));
            }

            index = Params.IndexOfOutputParam("length");
            if (index != -1)
            {
                List<ISegmentable3D> segmentable3Ds = sAMGeometries.ConvertAll(x => x as ISegmentable3D);
                segmentable3Ds.RemoveAll(x => x == null);
                double length = segmentable3Ds.ConvertAll(x => x.GetLength()).Sum();
                
                dataAccess.SetData(index, length);
            }
                
        }
    }
}