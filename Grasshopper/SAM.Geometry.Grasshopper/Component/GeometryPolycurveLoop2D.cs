using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryPolycurveLoop2D : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("27be7da1-8492-4400-9984-ffa70f3b5c69");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new Param_GenericObject() { Name = "_geometries", NickName = "_geometries", Description = "Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                Param_Boolean param_Boolean = new Param_Boolean() { Name = "_run_", NickName = "_run_", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.PersistentData.Append(new GH_Boolean(false));

                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Loops", NickName = "Lps", Description = "SAM Geometry Polygon2Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "InternalPoint", NickName = "IntrPt", Description = "Internal SAM Geometry Point2D", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "ExternalLoops", NickName = "ExtLps", Description = "External SAM Geometry Polygon2Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "InternalEdges", NickName = "IntEdgs", Description = "Internal/Adjacent Edges as SAM Geometry Segment2Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            bool run = false;

            index = Params.IndexOfInputParam("_run_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref run))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    dataAccess.SetData(3, false);
                    return;
                }
                if (!run)
                    return;
            }

            index = Params.IndexOfInputParam("_geometries");
            if (index == -1)
                return;

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
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
                        sAMGeometry3Ds.Add(Spatial.Query.Convert(Spatial.Plane.WorldXY, (ISAMGeometry2D)sAMGeometry));
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
                            curve3Ds.AddRange(((Spatial.ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Query.Project(Spatial.Plane.WorldXY, x)));
                        else if (sAMGeometry3D is Spatial.ICurve3D)
                            curve3Ds.Add(Spatial.Query.Project(Spatial.Plane.WorldXY, (Spatial.ICurve3D)sAMGeometry3D));

                        if (curve3Ds == null || curve3Ds.Count == 0)
                            continue;

                        foreach (Spatial.ICurve3D curve3D in curve3Ds)
                        {
                            if (curve3D == null)
                                continue;

                            ICurvable2D curvable2D = Spatial.Query.Convert(Spatial.Plane.WorldXY, curve3D) as ICurvable2D;
                            if (curvable2D != null)
                                curvable2D.GetCurves().ForEach(x => segment2Ds.Add(new Segment2D(x.GetStart(), x.GetEnd())));
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

            index = Params.IndexOfOutputParam("Loops");
            if (index != -1)
                dataAccess.SetDataList(index, polygon2Ds.ConvertAll(x => new GooSAMGeometry(x)));

            index = Params.IndexOfOutputParam("InternalPoint");
            if (index != -1)
                dataAccess.SetDataList(index, polygon2Ds.ConvertAll(x => new GooSAMGeometry(x.GetInternalPoint2D())));

            index = Params.IndexOfOutputParam("ExternalLoops");
            if (index != -1)
            {
                List<Polygon2D> polygon2Ds_External = Planar.Query.Union(polygon2Ds);
                dataAccess.SetDataList(index, polygon2Ds_External.ConvertAll(x => new GooSAMGeometry(x)));
            }

            index = Params.IndexOfOutputParam("InternalEdges");
            if (index != -1)
            {
                List<Segment2D> segment2Ds_External = Planar.Query.AdjacentSegment2Ds(polygon2Ds);
                dataAccess.SetDataList(index, segment2Ds_External.ConvertAll(x => new GooSAMGeometry(x)));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
                dataAccess.SetData(index, true);
        }
    }
}