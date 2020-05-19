using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometrySnapByPoints : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1165eb14-9f8a-460e-a3e7-e9b3d7f08ddc");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometrySnapByPoints()
          : base("Geometry.SnapByPoints", "GeometryEngine.SnapByPoints",
              "Snap Geometry or SAM Geometry by points",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "Geo", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_points", "Points", "snapping Points", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "mDis", "Max Distance to snap Geometry points", GH_ParamAccess.item, 1);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Geometry", "Geo", "modified SAM Geometry", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData<bool>(3, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Spatial.Point3D point3D = gHObjectWrapper.Value as Spatial.Point3D;
                if (point3D != null)
                {
                    point3Ds.Add(point3D);
                    continue;
                }

                GH_Point gHPoint = gHObjectWrapper.Value as GH_Point;
                if (gHPoint == null)
                    continue;

                point3Ds.Add(Convert.ToSAM(gHPoint));
            }

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            double maxDistance = double.NaN;

            GH_Number gHNumber = objectWrapper.Value as GH_Number;
            if (gHNumber != null)
                maxDistance = gHNumber.Value;

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

            List<Spatial.ISAMGeometry3D> geometry3Ds = null;

            if (obj is IGH_GeometricGoo)
                geometry3Ds = Convert.ToSAM((IGH_GeometricGoo)obj);
            else if (obj is Spatial.ISAMGeometry3D)
                geometry3Ds = new List<Spatial.ISAMGeometry3D>() { (Spatial.ISAMGeometry3D)obj };

            for (int i = 0; i < geometry3Ds.Count; i++)
            {
                if (geometry3Ds[i] is Spatial.Point3D)
                {
                    geometry3Ds[i] = Spatial.Point3D.Snap(point3Ds, (Spatial.Point3D)geometry3Ds[i], maxDistance);
                }
                else if (geometry3Ds[i] is Spatial.Segment3D)
                {
                    geometry3Ds[i] = Spatial.Segment3D.Snap(point3Ds, (Spatial.Segment3D)geometry3Ds[i], maxDistance);
                }
                else if (geometry3Ds[i] is Spatial.Polygon3D)
                {
                    geometry3Ds[i] = Spatial.Polygon3D.Snap(point3Ds, (Spatial.Polygon3D)geometry3Ds[i], maxDistance);
                }
            }

            dataAccess.SetData(0, geometry3Ds);
            dataAccess.SetData(1, true);

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}