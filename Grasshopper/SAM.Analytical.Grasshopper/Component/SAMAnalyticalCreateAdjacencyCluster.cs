using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyCluster : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("387683eb-fe77-4aa3-a71b-b25e613fdfcd");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAdjacencyCluster()
          : base("SAMAnalytical.CreateAdjacencyCluster", "SAMAnalytical.CreateAdjacencyCluster",
              "Create AdjacencyCluster",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddGenericParameter("_geometry", "_geometry", "SAM or Rhino Geometry", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddNumberParameter("elevation_Ground_", "elevation_Ground_", "Ground Elevation", GH_ParamAccess.item, 0);
            inputParamManager.AddNumberParameter("tolerance_", "tolerance_", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry> sAMGeometries = new List<ISAMGeometry>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrapperList)
            {
                if(objectWrapper.Value is ISAMGeometry)
                {
                    sAMGeometries.Add((ISAMGeometry)objectWrapper.Value);
                }
                else if(objectWrapper.Value is GooSAMGeometry)
                {
                    sAMGeometries.Add(((GooSAMGeometry)objectWrapper.Value).Value);
                }
                else if(objectWrapper.Value is IGH_GeometricGoo)
                {
                    object @object = Geometry.Grasshopper.Convert.ToSAM(objectWrapper.Value as dynamic);
                    if(@object is IEnumerable)
                        sAMGeometries.AddRange(((IEnumerable)@object).Cast<ISAMGeometry>());
                    else
                        sAMGeometries.Add(@object as dynamic);
                }
            }

            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(2, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMGeometries.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            List<ISegmentable3D> segmentable3Ds = new List<ISegmentable3D>();

            Geometry.Spatial.Plane plane_Default = Geometry.Spatial.Plane.WorldXY;
            foreach(ISAMGeometry sAMGeometry in sAMGeometries)
            {
                if (sAMGeometry is Face3D)
                {
                    face3Ds.Add((Face3D)sAMGeometry);
                    continue;
                }
                else if(sAMGeometry is ISegmentable3D)
                {
                    ISegmentable3D segmentable3D = (ISegmentable3D)sAMGeometry;
                    segmentable3Ds.Add(segmentable3D);

                }
                else if(sAMGeometry is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)sAMGeometry).GetCurves();
                    if(curve3Ds != null && curve3Ds.Count != 0)
                    {
                        if (curve3Ds.TrueForAll(x => x is Segment3D))
                        {
                            bool closed = curve3Ds.First().GetStart().AlmostEquals(curve3Ds.Last().GetEnd());
                            segmentable3Ds.Add(new Polyline3D(curve3Ds.Cast<Segment3D>(), closed));
                        }
                        else
                        {
                            foreach(ICurve3D curve3D in curve3Ds)
                                if (curve3D is ISegmentable3D)
                                    segmentable3Ds.Add((ISegmentable3D)curve3D);
                        }

                    }
                }
            }

            Dictionary<double, List<ISegmentable2D>> dictionary = new Dictionary<double, List<ISegmentable2D>>();
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                BoundingBox3D boundingBox3D = segmentable3D.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double elevation = Core.Query.Round(boundingBox3D.Min.Z, tolerance);

                Geometry.Spatial.Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, elevation)) as Geometry.Spatial.Plane;
                ISegmentable2D segmentable2D = plane.Convert(plane.Project(segmentable3D as dynamic));
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
                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(keyValuePair.Value);
                if (polygon2Ds == null)
                    continue;

                List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null)
                    continue;

                for(int i=0; i < face2Ds.Count; i++)
                {
                    List<IClosed2D> internalEdge2Ds = face2Ds[i]?.InternalEdge2Ds;
                    if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                        continue;

                    foreach(IClosed2D internalEdge2D in internalEdge2Ds)
                    {
                        if (internalEdge2D == null)
                            continue;

                        face2Ds.Add(new Face2D(internalEdge2D));
                    }
                }

                Geometry.Spatial.Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Geometry.Spatial.Plane;
                face3Ds.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
            }

            double elevation_Ground = double.NaN;
            if (!dataAccess.GetData(1, ref elevation_Ground))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(face3Ds, elevation_Ground, tolerance);

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}