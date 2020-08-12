using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
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

            List<SAMGeometry> sAMGeometries = new List<SAMGeometry>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrapperList)
            {
                if(objectWrapper.Value is SAMGeometry)
                {
                    sAMGeometries.Add((SAMGeometry)objectWrapper.Value);
                }
                else if(objectWrapper.Value is IGH_GeometricGoo)
                {
                    object @object = Geometry.Grasshopper.Convert.ToSAM(objectWrapper.Value as dynamic);
                    if(@object is IEnumerable)
                        sAMGeometries.AddRange(((IEnumerable)@object).Cast<SAMGeometry>());
                    else
                        sAMGeometries.Add(@object as dynamic);
                }
            }

            if (sAMGeometries.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            Dictionary<double, List<ISegmentable2D>> dictionary = new Dictionary<double, List<ISegmentable2D>>();

            Plane plane_Default = Plane.WorldXY;
            foreach(SAMGeometry sAMGeometry in sAMGeometries)
            {
                if (sAMGeometry is Face3D)
                {
                    face3Ds.Add((Face3D)sAMGeometry);
                    continue;
                }
                    
                if(sAMGeometry is ISegmentable3D)
                {
                    ISegmentable3D segmentable3D = (ISegmentable3D)sAMGeometry;
                    BoundingBox3D boundingBox3D = segmentable3D.GetBoundingBox();
                    if (boundingBox3D == null)
                        continue;

                    Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;
                    ISegmentable2D segmentable2D = plane.Convert(plane.Project(segmentable3D as dynamic));
                    if (segmentable2D == null)
                        continue;

                    List<ISegmentable2D> segmentable2Ds = null;
                    if (!dictionary.TryGetValue(plane.Origin.Z, out segmentable2Ds))
                    {
                        segmentable2Ds = new List<ISegmentable2D>();
                        dictionary[plane.Origin.Z] = segmentable2Ds;
                    }

                    segmentable2Ds.Add(segmentable2D);
                    continue;
                }
            }

            foreach(KeyValuePair<double, List<ISegmentable2D>> keyValuePair in dictionary)
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

                Plane plane = plane_Default.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Plane;
                face3Ds.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
            }

            double elevation_Ground = double.NaN;
            if (!dataAccess.GetData(1, ref elevation_Ground))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(face3Ds, elevation_Ground, Core.Tolerance.MacroDistance);

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}