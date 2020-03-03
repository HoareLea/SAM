using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper.Properties;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryPolycurveLoop2D : GH_Component
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
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Geometry", "Geo", "modified SAM Geometry", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Center", "Center", "Center Point", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData<bool>(1, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(2, false);
                return;
            }
            if (!run)
                return;

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(2, false);
                return;
            }

            List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Planar.Segment2D segment2D = gHObjectWrapper.Value as Planar.Segment2D;
                if (segment2D != null)
                {
                    segment2Ds.Add(segment2D);
                    continue;
                }

                if (gHObjectWrapper.Value is IGH_GeometricGoo)
                {
                    List<Spatial.ISAMGeometry3D> sAMGeometry3Ds = Convert.ToSAM((IGH_GeometricGoo)gHObjectWrapper.Value);
                    foreach (Spatial.ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                    {
                        List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
                        if (sAMGeometry3D is Spatial.ICurvable3D)
                            curve3Ds.AddRange(((Spatial.ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Plane.Base.Project(x)));
                        else if (sAMGeometry3D is Spatial.ICurve3D)
                            curve3Ds.Add(Spatial.Plane.Base.Project((Spatial.ICurve3D)sAMGeometry3D));

                        if (curve3Ds == null || curve3Ds.Count == 0)
                            continue;

                        foreach (Spatial.ICurve3D curve3D in curve3Ds)
                        {
                            if (curve3D == null)
                                continue;

                            Planar.ICurvable2D curvable2D = Spatial.Plane.Base.Convert(curve3D) as Planar.ICurvable2D;
                            if (curvable2D != null)
                                curvable2D.GetCurves().ForEach(x => segment2Ds.Add(new Planar.Segment2D(x.GetStart(), x.GetEnd())));
                        }
                    }
                }
            }

            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
            }

            segment2Ds = Planar.Modify.Split(segment2Ds);

            Planar.CurveGraph2D curveGraph2D = new Planar.CurveGraph2D(segment2Ds);
            List<Planar.PolycurveLoop2D> polycurveLoop2Ds = curveGraph2D.GetPolycurveLoop2Ds();
            dataAccess.SetDataList(0, polycurveLoop2Ds.ConvertAll(x => new GooSAMGeometry(x)));
            dataAccess.SetDataList(1, polycurveLoop2Ds.ConvertAll(x => new GooSAMGeometry(x.GetCentroid())));
            dataAccess.SetData(2, true);

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}