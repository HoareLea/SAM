using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySplit : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5de5aeeb-7d6c-4598-bf45-df9d72edb8f5");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySplit()
          : base("SAMGeometry.Split", "SAMGeometry.Split",
              "Split Geometry or Segment2Ds",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "Geo", "Geometry", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_tolerance_", "tolrnc", "Split Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
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
            if (!dataAccess.GetData<bool>(2, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(1, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(3, false);
                return;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Segment2D segment2D = gHObjectWrapper.Value as Segment2D;
                if (segment2D != null)
                {
                    segment2Ds.Add(segment2D);
                    continue;
                }

                List<Spatial.ISAMGeometry3D> sAMGeometry3Ds = null;
                if (gHObjectWrapper.Value is GooSAMGeometry)
                {
                    sAMGeometry3Ds = new List<Spatial.ISAMGeometry3D>();

                    ISAMGeometry sAMGeometry = ((GooSAMGeometry)gHObjectWrapper.Value).Value;
                    if (sAMGeometry is Spatial.ISAMGeometry3D)
                        sAMGeometry3Ds.Add((Spatial.ISAMGeometry3D)sAMGeometry);
                    else if (sAMGeometry is ISAMGeometry2D)
                        sAMGeometry3Ds.Add(Spatial.Plane.WorldXY.Convert((ISAMGeometry2D)sAMGeometry));
                }
                else if (gHObjectWrapper.Value is IGH_GeometricGoo)
                {
                    sAMGeometry3Ds = Convert.ToSAM((IGH_GeometricGoo)gHObjectWrapper.Value);
                }

                if (sAMGeometry3Ds != null && sAMGeometry3Ds.Count > 0)
                {
                    foreach (Spatial.ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                    {
                        List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
                        if (sAMGeometry3D is Spatial.ICurvable3D)
                            curve3Ds.AddRange(((Spatial.ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Plane.WorldXY.Project(x)));
                        else if (sAMGeometry3D is Spatial.ICurve3D)
                            curve3Ds.Add(Spatial.Plane.WorldXY.Project((Spatial.ICurve3D)sAMGeometry3D));

                        if (curve3Ds == null || curve3Ds.Count == 0)
                            continue;

                        foreach (Spatial.ICurve3D curve3D in curve3Ds)
                        {
                            if (curve3D == null)
                                continue;

                            ICurvable2D curvable2D = Spatial.Plane.WorldXY.Convert(curve3D) as ICurvable2D;
                            if (curvable2D != null)
                                curvable2D.GetCurves().ForEach(x => segment2Ds.Add(new Segment2D(x.GetStart(), x.GetEnd())));
                        }
                    }
                }
            }

            dataAccess.SetDataList(0, segment2Ds.Split(tolerance));
            dataAccess.SetData(1, true);
        }
    }
}