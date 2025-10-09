using Grasshopper.Kernel;
using NetTopologySuite.Geometries;
using SAM.Analytical.Classes;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSpaceLoactionPoint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("14da369b-312a-448c-b69a-3ea69ae7563c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces, if nothing connected all spaces from AnalyticalModel will be used", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Cutting plane offset from the bottom of space", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "aspect_", NickName = "aspect_", Description = "Aspect", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(2.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "clerance_", NickName = "clerance_", Description = "Clerance", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.15);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heightFactor_", NickName = "heightFactor_", Description = "Height Factor", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));


                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() {Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Heights", NickName = "Heights", Description = "SAM Heights", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                return [.. result];
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateSpaceLoactionPoint()
          : base("SAMAnalytical.UpdateSpaceLoactionPoint", "SAMAnalytical.UpdateSpaceLoactionPoint",
              "Updates Space Loaction Point",
              "SAM", "SAM_IC")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analyticalObject");
            IAnalyticalObject? analyticalObject = null;
            if(index == -1 || !dataAccess.GetData(index, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(analyticalObject is AdjacencyCluster adjacencyCluster_Temp)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster_Temp, true);
            }
            else if(analyticalObject is AnalyticalModel analyticalModel)
            {
                adjacencyCluster = new AdjacencyCluster(analyticalModel.AdjacencyCluster, true);
            }

            if(adjacencyCluster is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("spaces_");
            List<Space> spaces = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            index = Params.IndexOfInputParam("offset_");
            double offset = 0.1;
            if (index != -1)
            {
                dataAccess.GetData(index, ref offset);
            }

            index = Params.IndexOfInputParam("heightFactor_");
            double heightFactor = 1;
            if (index != -1)
            {
                dataAccess.GetData(index, ref heightFactor);
            }

            index = Params.IndexOfInputParam("aspect_");
            double aspect = 2.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref aspect);
            }

            index = Params.IndexOfInputParam("clerance_");
            double clerance = 0.15;
            if (index != -1)
            {
                dataAccess.GetData(index, ref clerance);
            }

            if (spaces == null || spaces.Count == 0)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            List<double> heights = [];
            List<ISpace> spaces_Result = [];

            if(spaces != null && spaces.Count != 0)
            {
                foreach (Space space in spaces)
                {
                    if(adjacencyCluster.GetObject<Space>(space.Guid) is not Space space_Temp)
                    {
                        continue;
                    }

                    Shell shell = adjacencyCluster.Shell(space);
                    if (shell is null)
                    {
                        continue;
                    }

                    Plane plane = Geometry.Spatial.Create.Plane(shell.GetBoundingBox().Min.Z + offset);

                    Dictionary<Face3D, List<ISegmentable2D>> dictionary = Geometry.Spatial.Query.SectionDictionary<ISegmentable2D>(shell.Face3Ds, plane);

                    List<Segment2D> segment2Ds = [];
                    foreach (KeyValuePair<Face3D, List<ISegmentable2D>> keyValuePair in dictionary)
                    {
                        foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                        {
                            segment2Ds.AddRange(segmentable2D.GetSegments());
                        }
                    }

                    if(segment2Ds is null || segment2Ds.Count == 0)
                    {
                        continue;
                    }

                    segment2Ds = Geometry.Planar.Query.Split(segment2Ds);
                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds);
                    if (polygon2Ds == null || polygon2Ds.Count == 0)
                    {
                        continue;
                    }

                    List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Opposite);
                    if (face2Ds == null || face2Ds.Count == 0)
                    {
                        continue;
                    }

                    foreach(Face2D face2D in face2Ds)
                    {
                        (Coordinate center, double width, double height) coordinate = PolygonLabelCoordinate.LargestAxisAlignedRectangle(face2D.ToNTS(), aspect, clerance);


                        Point2D point2D = coordinate.center.ToSAM();

                        if(plane.Convert(point2D) is not Point3D point3D)
                        {
                            continue;
                        }

                        Point3D location = new Point3D(point3D.X, point3D.Y, space.Location.Z);
                        space_Temp = new Space(space_Temp, space_Temp.Name, location);
                        adjacencyCluster.AddObject(space_Temp);
                        
                        spaces_Result.Add(space_Temp);
                        heights.Add(coordinate.height * heightFactor);

                        break;
                    }

                }
            }

            if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(analyticalObject));
            }

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces_Result);
            }

            index = Params.IndexOfOutputParam("Heights");
            if (index != -1)
            {
                dataAccess.SetDataList(index, heights);
            }
        }
    }
}