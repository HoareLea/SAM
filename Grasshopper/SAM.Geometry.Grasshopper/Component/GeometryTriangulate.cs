using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryTriangulate : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9c8e9b12-e698-4a31-a389-fab330832d2b");

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
        public GeometryTriangulate()
          : base("Geometry.Triangulate", "Geometry.Triangulate",
              "Triangulate Geometry",
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

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "_geometry", Description = "SAM Geometry", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "points_", NickName = "points_", Description = "SAM Point3Ds", Access = GH_ParamAccess.list };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "mesh3D", NickName = "mesh3D", Description = "SAM Geometry Mesh3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "triangle3Ds", NickName = "triangle3Ds", Description = "SAM Geometry Triangle3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_geometry");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Point3D> point3Ds = null;

            index = Params.IndexOfInputParam("points_");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index != -1 && dataAccess.GetDataList(index, objectWrappers) && objectWrappers != null && objectWrappers.Count != 0)
            {
                point3Ds = new List<Point3D>();

                foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
                {
                    if (Query.TryGetSAMGeometries(objectWrapper_Temp, out List<Point3D> point3Ds_Temp) && point3Ds_Temp != null && point3Ds_Temp.Count > 0)
                        point3Ds.AddRange(point3Ds_Temp);
                }
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<Triangle3D> triangle3Ds = new List<Triangle3D>();
            foreach(Face3D face3D in face3Ds)
            {
                if(face3D == null)
                {
                    continue;
                }

                List<Triangle3D> triangle3Ds_Temp = face3D.Triangulate(point3Ds, tolerance);
                if (triangle3Ds_Temp != null && triangle3Ds_Temp.Count > 0)
                {
                    triangle3Ds.AddRange(triangle3Ds_Temp);
                }
            }

            index = Params.IndexOfOutputParam("mesh3D");
            if (index != -1)
            {
                dataAccess.SetData(index, Spatial.Create.Mesh3D(triangle3Ds, tolerance));
            }

            index = Params.IndexOfOutputParam("triangle3Ds");
            if (index != -1)
            {
                dataAccess.SetDataList(index, triangle3Ds);
            }

        }
    }
}