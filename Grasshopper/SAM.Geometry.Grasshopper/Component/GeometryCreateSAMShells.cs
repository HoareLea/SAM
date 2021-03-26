using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryCreateSAMShells : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6de25be1-4ffe-400b-8110-6815cb932b66");

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
        public GeometryCreateSAMShells()
          : base("Geometry.CreateSAMShells", "Geometry.CreateSAMShells",
              "Create Shells ",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometries", NickName = "_geometries", Description = "Geometries", Access = GH_ParamAccess.list };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Shells", NickName = "Shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_geometries");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry> sAMGeometries = new List<ISAMGeometry>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (objectWrapper == null || objectWrapper.Value == null)
                    continue;

                if (objectWrapper.Value is ISAMGeometry)
                {
                    sAMGeometries.Add((ISAMGeometry)objectWrapper.Value);
                }
                else if (objectWrapper.Value is GooSAMGeometry)
                {
                    sAMGeometries.Add(((GooSAMGeometry)objectWrapper.Value).Value);
                }
                else if (objectWrapper.Value is IGH_GeometricGoo)
                {
                    object @object = Convert.ToSAM(objectWrapper.Value as dynamic);
                    if (@object is IEnumerable)
                        sAMGeometries.AddRange(((IEnumerable)@object).Cast<ISAMGeometry>());
                    else
                        sAMGeometries.Add(@object as dynamic);
                }
            }

            if (sAMGeometries.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            List<ISegmentable3D> segmentable3Ds = new List<ISegmentable3D>();

            Plane plane_Default = Plane.WorldXY;
            foreach (ISAMGeometry sAMGeometry in sAMGeometries)
            {
                if (sAMGeometry is Face3D)
                {
                    face3Ds.Add((Face3D)sAMGeometry);
                    continue;
                }
                else if (sAMGeometry is ISegmentable3D)
                {
                    ISegmentable3D segmentable3D = (ISegmentable3D)sAMGeometry;
                    segmentable3Ds.Add(segmentable3D);

                }
                else if (sAMGeometry is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)sAMGeometry).GetCurves();
                    if (curve3Ds != null && curve3Ds.Count != 0)
                    {
                        if (curve3Ds.TrueForAll(x => x is Segment3D))
                        {
                            bool closed = curve3Ds.First().GetStart().AlmostEquals(curve3Ds.Last().GetEnd());
                            segmentable3Ds.Add(new Polyline3D(curve3Ds.Cast<Segment3D>(), closed));
                        }
                        else
                        {
                            foreach (ICurve3D curve3D in curve3Ds)
                                if (curve3D is ISegmentable3D)
                                    segmentable3Ds.Add((ISegmentable3D)curve3D);
                        }

                    }
                }
            }

            Dictionary<double, List<ISegmentable2D>> dictionary = new Dictionary<double, List<ISegmentable2D>>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                BoundingBox3D boundingBox3D = segmentable3D.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double elevation = Core.Query.Round(boundingBox3D.Min.Z, tolerance);

                Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, elevation)) as Plane;
                ISegmentable2D segmentable2D = Spatial.Query.Convert(plane, Spatial.Query.Project(plane, segmentable3D as dynamic));
                if (segmentable2D == null)
                    continue;

                List<ISegmentable2D> segmentable2Ds = null;
                if (!dictionary.TryGetValue(elevation, out segmentable2Ds))
                {
                    segmentable2Ds = new List<ISegmentable2D>();
                    dictionary[elevation] = segmentable2Ds;
                }

                segmentable2Ds.Add(segmentable2D);
                continue;
            }

            foreach (KeyValuePair<double, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                    segment2Ds.AddRange(segmentable2D.GetSegments());

                segment2Ds = Planar.Query.Snap(segment2Ds, true);

                List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance);
                if (polygon2Ds == null)
                    continue;

                List<Face2D> face2Ds = Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null)
                    continue;

                for (int i = 0; i < face2Ds.Count; i++)
                {
                    List<IClosed2D> internalEdge2Ds = face2Ds[i]?.InternalEdge2Ds;
                    if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                        continue;

                    internalEdge2Ds.RemoveAll(x => x is Polygon2D && face2Ds.Find(y => y.ExternalEdge2D is Polygon2D && ((Polygon2D)y.ExternalEdge2D).Similar((Polygon2D)x)) != null);

                    foreach (IClosed2D internalEdge2D in internalEdge2Ds)
                    {
                        if (internalEdge2D == null)
                            continue;

                        Face2D face2D = face2Ds.Find(x => x.Inside(internalEdge2D.InternalPoint2D()));
                        if (face2D == null)
                            face2Ds.Add(new Face2D(internalEdge2D));
                    }
                }

                Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Plane;
                face3Ds.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
            }

            //List<Shell> shells = Spatial.Create.Shells_Depreciated(face3Ds, Core.Tolerance.MacroDistance, tolerance);
            List<Shell> shells = Spatial.Create.Shells(face3Ds, tolerance);

            index = Params.IndexOfInputParam("Shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells?.ConvertAll(x => new GooSAMGeometry(x)));
        }
    }
}