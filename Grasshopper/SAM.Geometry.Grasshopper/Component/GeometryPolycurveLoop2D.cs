using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryPolycurveLoop2D : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("27be7da1-8492-4400-9984-ffa70f3b5c69");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometryPolycurveLoop2D()
          : base("Geometry.PolycurveLoop2D", "Geometry.PolycurveLoop2D",
              "Find PolycurveLoop2Ds in geometry",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "Geo", "Geometry", GH_ParamAccess.list);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Loops", "Lps", "SAM PolycurveLoop2Ds", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "InternalPoint", "IntrPt", "Internal Point", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "ExternalLoops", "ExtLps", "SAM External PolycurveLoop2Ds", GH_ParamAccess.list);
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
            if (!dataAccess.GetData<bool>(1, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(3, false);
                return;
            }
            if (!run)
                return;

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
                    else if (sAMGeometry is Planar.ISAMGeometry2D)
                        sAMGeometry3Ds.Add(Spatial.Plane.WorldXY.Convert((Planar.ISAMGeometry2D)sAMGeometry));
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

                            Planar.ICurvable2D curvable2D = Spatial.Plane.WorldXY.Convert(curve3D) as Planar.ICurvable2D;
                            if (curvable2D != null)
                                curvable2D.GetCurves().ForEach(x => segment2Ds.Add(new Planar.Segment2D(x.GetStart(), x.GetEnd())));
                        }
                    }
                }
            }

            if (segment2Ds == null || segment2Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
            }

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds);

            dataAccess.SetDataList(0, polygon2Ds.ConvertAll(x => new GooSAMGeometry(x)));
            dataAccess.SetDataList(1, polygon2Ds.ConvertAll(x => new GooSAMGeometry(x.GetInternalPoint2D())));

            List<Polygon2D> polygon2Ds_External = Planar.Query.Union(polygon2Ds);
            dataAccess.SetDataList(2, polygon2Ds_External.ConvertAll(x => new GooSAMGeometry(x)));

            dataAccess.SetData(3, true);
        }
    }
}