using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryPlaneIntersection : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("02c2bed1-ef27-48be-bcf0-dc969d0b6d90");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryPlaneIntersection()
          : base("SAMGeometry.PlaneIntersection", "GHgeo",
              "Plane Intersection",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry3D", "_geometry3D", "SAM Geometry 3D", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_plane", "_plane", "SAM Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Geometries", "Geometries", "Intersection Geometries", GH_ParamAccess.list);
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

            objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            object obj = objectWrapper.Value;
            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;

            if (obj is IGH_GeometricGoo)
                geometry3Ds = Convert.ToSAM((IGH_GeometricGoo)obj);
            else if (obj is ISAMGeometry3D)
                geometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)obj };
            else if (obj is GooSAMGeometry)
                geometry3Ds = new List<ISAMGeometry3D>() { ((GooSAMGeometry)obj).Value as ISAMGeometry3D };

            objectWrapper = null;
            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            Plane plane = null;

            if (objectWrapper.Value is Plane)
                plane = objectWrapper.Value as Plane;
            else if (objectWrapper.Value is GooSAMGeometry)
                plane = ((GooSAMGeometry)objectWrapper.Value).Value as Plane;
            else if (objectWrapper.Value is GH_Plane)
                plane = ((GH_Plane)objectWrapper.Value).ToSAM();

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
            foreach (Spatial.ISAMGeometry3D geometry3D in geometry3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = plane.Intersection(geometry3D as dynamic);
                if (planarIntersectionResult == null)
                    continue;

                List<ISAMGeometry3D> geometry3Ds_Temp = planarIntersectionResult.Geometry3Ds;
                if (geometry3Ds_Temp != null && geometry3Ds_Temp.Count > 0)
                    result.AddRange(geometry3Ds_Temp);
            }

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooSAMGeometry(x)));
            dataAccess.SetData(1, true);
        }
    }
}