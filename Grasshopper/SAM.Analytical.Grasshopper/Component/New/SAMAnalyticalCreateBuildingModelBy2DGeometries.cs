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
    public class SAMAnalyticalCreateBuildingModelBy2DGeometries : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d8231ba9-c20c-44e0-a51e-5aa42da8c939");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateBuildingModelBy2DGeometries()
          : base("SAMAnalytical.CreateBuildingModelBy2DGeometries(", "SAMAnalytical.CreateBuildingModelBy2DGeometries",
              "Creates BuildingModel By 2D Geometries of the floors and roofs",
              "SAM WIP", "Analytical")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometries", NickName = "_geometries", Description = "2D Geometries", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_groundElevation_", NickName = "_groundElevation_", Description = "Ground Elevation", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                GooLocationParam locationParam = new GooLocationParam() { Name = "_location_", NickName = "_location_", Description = "SAM Core Location", Access = GH_ParamAccess.item};
                locationParam.SetPersistentData(Core.Query.DefaultLocation());
                result.Add(new GH_SAMParam(locationParam, ParamVisibility.Voluntary));

                GooAddressParam addressParam = new GooAddressParam() { Name = "_address_", NickName = "_address_", Description = "SAM Core Address", Access = GH_ParamAccess.item };
                addressParam.SetPersistentData(Core.Query.DefaultAddress());
                result.Add(new GH_SAMParam(addressParam, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));


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
                result.Add(new GH_SAMParam(new GooBuildingModelParam() { Name = "buildingModel", NickName = "buildingModel", Description = "SAM Architectural BuildingModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_geometries");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
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
                    object @object = Geometry.Grasshopper.Convert.ToSAM(objectWrapper.Value as dynamic);
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

            double tolerance = Core.Tolerance.Distance;
            index = Params.IndexOfInputParam("tolerance_");
            if(index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (dataAccess.GetData(index, ref tolerance_Temp))
                {
                    tolerance = tolerance_Temp;
                }
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
                ISegmentable2D segmentable2D = Geometry.Spatial.Query.Convert(plane, Geometry.Spatial.Query.Project(plane, segmentable3D as dynamic));
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
                IEnumerable<ISegmentable2D> segmentable2Ds = Geometry.Planar.Query.Split(keyValuePair.Value, tolerance)?.Cast<ISegmentable2D>();
                if (segmentable2Ds == null)
                    segmentable2Ds = keyValuePair.Value;

                if (segmentable2Ds == null)
                    continue;

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D in segmentable2Ds)
                    segment2Ds.AddRange(segmentable2D.GetSegments());

                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds);
                //List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(keyValuePair.Value);
                if (polygon2Ds == null)
                    continue;

                List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Opposite);
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

            double elevation_Ground = Core.Tolerance.Distance;
            index = Params.IndexOfInputParam("_groundElevation_");
            if (index != -1)
            {
                double elevation_Ground_Temp = double.NaN;
                if (dataAccess.GetData(index, ref elevation_Ground_Temp))
                {
                    elevation_Ground = elevation_Ground_Temp;
                }
            }

            index = Params.IndexOfInputParam("_location_");
            Core.Location location = Core.Query.DefaultLocation();
            if (index != -1)
                dataAccess.GetData(index, ref location);

            index = Params.IndexOfInputParam("_address_");
            Core.Address address = Core.Query.DefaultAddress();
            if (index != -1)
                dataAccess.GetData(index, ref address);

            index = Params.IndexOfOutputParam("buildingModel");
            if (index != -1)
            {
                BuildingModel buildingModel = Create.BuildingModel(face3Ds, elevation_Ground, tolerance);
                if(buildingModel != null)
                {
                    buildingModel.Address = address;
                    buildingModel.Location = location;
                }

                dataAccess.SetData(index, buildingModel != null ? new GooBuildingModel(buildingModel) : null);
            }
        }
    }
}