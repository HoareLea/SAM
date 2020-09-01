using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double elevation_Ground = 0, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Plane plane_Min = Plane.WorldXY.GetMoved(new Vector3D(0,0, elevation_Min)) as Plane;
            Plane plane_Max = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

            Plane plane_Min_Flipped = new Plane(plane_Min);
            plane_Min_Flipped.FlipZ();

            Construction construction_Wall = Query.Construction(PanelType.Wall);
            Construction construction_Floor = Query.Construction(PanelType.Floor);
            Construction construction_FloorExposed = Query.Construction(PanelType.FloorExposed);
            Construction construction_Roof = Query.Construction(PanelType.Roof);

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
            if(polygon2Ds != null && polygon2Ds.Count != 0)
            {
                List<Tuple<Segment2D, Panel, Space>> tuples = new List<Tuple<Segment2D, Panel, Space>>();
                for(int i=0; i < polygon2Ds.Count; i++)
                {
                    Polygon2D polygon2D = polygon2Ds[i];
                    Space space = new Space(string.Format("Cell {0}", i + 1), plane_Min.Convert(polygon2D.GetInternalPoint2D()));
                    adjacencyCluster.AddObject(space);

                    List<Segment2D> segment2Ds = polygon2D.GetSegments();
                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds.SimplifyByAngle();
                    Geometry.Planar.Modify.Snap(segment2Ds, true, tolerance);

                    foreach(Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                        Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });
                        Panel panel = new Panel(construction_Wall, PanelType.Wall, new Face3D(polygon3D));

                        tuples.Add(new Tuple<Segment2D, Panel, Space> (segment2D, panel, space));
                    }

                    Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                    polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));
                    if (polygon3D_Min != null)
                    {
                        Construction construction_Floor_Temp = construction_Floor;
                        if (polygon3D_Min.GetBoundingBox().Min.Z > elevation_Ground)
                            construction_Floor_Temp = construction_FloorExposed;

                        Panel panel_Min = new Panel(construction_Floor_Temp, PanelType.Floor, new Face3D(polygon3D_Min));
                        adjacencyCluster.AddObject(panel_Min);
                        adjacencyCluster.AddRelation(space, panel_Min);
                    }

                    Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                    if (polygon3D_Max != null)
                    {
                        Panel panel_Max = new Panel(construction_Roof, PanelType.Roof, new Face3D(polygon3D_Max));
                        adjacencyCluster.AddObject(panel_Max);
                        adjacencyCluster.AddRelation(space, panel_Max);
                    }
                }

                while(tuples.Count > 0)
                {
                    Tuple<Segment2D, Panel, Space> tuple = tuples[0];
                    Segment2D segment2D = tuple.Item1;

                    List<Tuple<Segment2D, Panel, Space>> tuples_Temp = tuples.FindAll(x => segment2D.AlmostSimilar(x.Item1));
                    tuples.RemoveAll(x => tuples_Temp.Contains(x));

                    Panel panel = tuple.Item2;
                    adjacencyCluster.AddObject(panel);

                    foreach (Tuple<Segment2D, Panel, Space> tuple_Temp in tuples_Temp)
                        adjacencyCluster.AddRelation(tuple_Temp.Item3, panel);
                }
            }

            adjacencyCluster.UpdatePanelTypes(elevation_Ground);
            adjacencyCluster.UpdateConstructions();

            return adjacencyCluster;
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Face3D> face3Ds, double elevation_Ground = 0, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            Plane plane_Default = Plane.WorldXY;
            plane_Default.Move(new Vector3D(0,0, elevation_Ground));

            Dictionary<Face2D, Face3D> dictionary_Project = new Dictionary<Face2D, Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                Face3D face3D_Project = plane_Default.Project(face3D);
                if (face3D_Project == null)
                    continue;

                Face2D face2D = plane_Default.Convert(face3D_Project);
                if (face2D == null)
                    continue;

                dictionary_Project[face2D] = face3D;
            }

            if (dictionary_Project.Count == 0)
                return null;

            List<Face2D> face2Ds = Geometry.Planar.Query.Union(dictionary_Project.Keys);
            if (face2Ds == null)
                return null;

            Dictionary<Face2D, List<Face3D>> dictionary_Common = new Dictionary<Face2D, List<Face3D>>();
            foreach(KeyValuePair<Face2D, Face3D> keyValuePair_Project in dictionary_Project)
            {
                Face2D face2D = null;
                foreach(Face2D face2D_Temp in face2Ds)
                {
                    if(face2D_Temp.InRange(keyValuePair_Project.Key.GetInternalPoint2D(), tolerance))
                    {
                        face2D = face2D_Temp;
                        break;
                    }
                }

                if (face2D == null)
                    continue;

                if (!dictionary_Common.ContainsKey(face2D))
                    dictionary_Common[face2D] = new List<Face3D>();
                
                dictionary_Common[face2D].Add(keyValuePair_Project.Value);
            }

            List<AdjacencyCluster> adjacencyClusters = new List<AdjacencyCluster>();
            foreach(List<Face3D> face3Ds_Common in dictionary_Common.Values)
            {
                Dictionary<double, List<Face2D>> dictionary = new Dictionary<double, List<Face2D>>();
                foreach(Face3D face3D in face3Ds_Common)
                {
                    BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                    if (boundingBox3D == null)
                        continue;

                    double elevation = SAM.Core.Query.Round(boundingBox3D.Min.Z, tolerance);
                    plane_Default.Move(new Vector3D(0, 0, elevation));

                    Face2D face2D = plane_Default.Convert(plane_Default.Project(face3D));
                    if (face2D == null)
                        continue;

                    List<Face2D> face2Ds_Elevation = null;
                    if(!dictionary.TryGetValue(elevation, out face2Ds_Elevation))
                    {
                        face2Ds_Elevation = new List<Face2D>();
                        dictionary[elevation] = face2Ds_Elevation;
                    }

                    face2Ds_Elevation.Add(face2D);
                }

                List<double> elevations = dictionary.Keys.ToList();
                if(!elevations.Contains(elevation_Ground))
                {
                    elevations.Sort();
                    for(int i=1; i < elevations.Count; i++)
                    {
                        if (elevation_Ground > elevations[i])
                            continue;

                        dictionary[elevation_Ground] = dictionary[elevations[i - 1]];
                        break;
                    }

                    if(!dictionary.ContainsKey(elevation_Ground))
                        dictionary[elevation_Ground] = dictionary[elevations.Last()];
                }

                AdjacencyCluster adjacencyCluster = AdjacencyCluster(dictionary, dictionary.Keys.ToList().IndexOf(elevation_Ground), tolerance);
                if (adjacencyCluster != null)
                    adjacencyClusters.Add(adjacencyCluster);
            }

            AdjacencyCluster result = new AdjacencyCluster();
            foreach (AdjacencyCluster adjacencyCluster in adjacencyClusters)
                result.Join(adjacencyCluster);

            return result;
        }

        private static AdjacencyCluster AdjacencyCluster(this Dictionary<double, List<Face2D>> face2Ds, int index_Ground, double tolerance = Core.Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() < 2)
                return null;

            List<double> elevations = face2Ds.Keys.ToList();
            elevations.Sort((x, y) => y.CompareTo(x));

            double elevation_Ground = face2Ds.Keys.ElementAt(index_Ground);

            List<Tuple<double, List<Face2D>>> tuples = new List<Tuple<double, List<Face2D>>>();

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[0], face2Ds[elevations[0]]));

            for(int i=1; i < elevations.Count - 1; i++)
            {
                double elevation_Top = elevations[i - 1];
                double elevation_Bottom = elevations[i];

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];
                List<Face2D> face2Ds_Bottom = face2Ds[elevation_Bottom];

                List<IClosed2D> closed2Ds = new List<IClosed2D>();

                if (face2Ds_Top != null && face2Ds_Top.Count > 0)
                    face2Ds_Top.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                if (face2Ds_Bottom != null && face2Ds_Bottom.Count > 0)
                    face2Ds_Bottom.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
                foreach(IClosed2D closed2D in closed2Ds)
                {
                    ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                    if (segmentable2D == null)
                        continue;

                    segmentable2Ds.Add(segmentable2D);
                }

                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Face2D> face2Ds_New = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);

                List<Face2D> face2Ds_All = new List<Face2D>();
                face2Ds_All.AddRange(face2Ds_Top);
                face2Ds_All.AddRange(face2Ds_Bottom);

                List<IClosed2D> internalEdge2Ds = new List<IClosed2D>();
                foreach(Face2D face2D_New in face2Ds_New)
                {
                    List<IClosed2D> internalEdge2Ds_New = face2D_New.InternalEdge2Ds;
                    if (internalEdge2Ds_New == null || internalEdge2Ds_New.Count == 0)
                        continue;

                    foreach (IClosed2D closed2D in internalEdge2Ds_New)
                    {
                        Point2D point2D = closed2D?.GetInternalPoint2D();
                        if (point2D == null)
                            continue;

                        if (face2Ds_All.Find(x => x.Inside(point2D)) == null)
                            continue;

                        internalEdge2Ds.Add(closed2D);
                    }
                }

                internalEdge2Ds.RemoveAll(x => x is Polygon2D && face2Ds_New.Find(y => y.ExternalEdge2D is Polygon2D && ((Polygon2D)y.ExternalEdge2D).Similar((Polygon2D)x)) != null);

                face2Ds_New.AddRange(internalEdge2Ds.ConvertAll(x => new Face2D(x)));

                tuples.Add(new Tuple<double, List<Face2D>>(elevation_Bottom, face2Ds_New));
            }

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[elevations.Count - 1], face2Ds[elevations[elevations.Count - 1]]));

            Construction construction_Wall = Query.Construction(PanelType.Wall);
            Construction construction_Floor = Query.Construction(PanelType.Floor);
            Construction construction_Roof = Query.Construction(PanelType.Roof);

            Plane plane = Plane.WorldXY;

            List<Tuple<Point3D, Panel, Space>> tuples_Point3D = new List<Tuple<Point3D, Panel, Space>>();

            AdjacencyCluster result = new AdjacencyCluster();

            for (int i = 1; i < tuples.Count; i++)
            {
                Tuple<double, List<Face2D>> tuple_Top = tuples[i - 1];
                Tuple<double, List<Face2D>> tuple_Bottom = tuples[i];

                double elevation_Top = tuple_Top.Item1;
                double elevation_Bottom = tuple_Bottom.Item1;

                Plane plane_Top = plane.GetMoved(new Vector3D(0, 0, elevation_Top)) as Plane;
                Plane plane_Bottom = plane.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Plane;
                Plane plane_Bottom_Flipped = new Plane(plane_Bottom);
                plane_Bottom_Flipped.FlipZ();

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];

                int count = 1;
                foreach (Face2D face2D in face2Ds_Top)
                {
                    Space space = new Space(string.Format("Cell {0}.{1}", i, count), plane_Bottom.Convert(face2D.GetInternalPoint2D()));
                    count++;

                    if (space != null)
                        result.AddObject(space);

                    List<Segment2D> segment2Ds = new List<Segment2D>();
                    foreach(IClosed2D closed2D in face2D.Edge2Ds)
                    {
                        ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                        if (segmentable2D == null)
                            continue;

                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }

                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds.SimplifyByAngle();
                    Geometry.Planar.Modify.Snap(segment2Ds, true, tolerance);

                    Panel panel;

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Top = plane_Top.Convert(segment2D);
                        Segment3D segment3D_Bottom = plane_Bottom.Convert(segment2D);
                        
                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] });
                        panel = new Panel(construction_Wall, PanelType.Wall, new Face3D(polygon3D));

                        tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(Geometry.Spatial.Query.Mid(plane_Bottom.Convert(segment2D)), panel, space));
                    }

                    List<Face2D> face2Ds_Space_Top = tuple_Top.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    foreach(Face2D face2D_Top in face2Ds_Space_Top)
                    {
                        Face3D face3D_Top = plane_Top.Convert(face2D_Top);
                        if (face3D_Top == null)
                            continue;

                        panel = new Panel(construction_Roof, PanelType.Roof, face3D_Top);
                        tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(face3D_Top.InternalPoint3D(), panel, space));
                    }

                    List<Face2D> face2Ds_Space_Bottom = tuple_Bottom.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    if (face2Ds_Space_Bottom != null && face2Ds_Space_Bottom.Count != 0)
                    {
                        foreach (Face2D face2D_Bottom in face2Ds_Space_Bottom)
                        {
                            Face3D face3D_Bottom = plane_Bottom.Convert(face2D_Bottom);
                            if (face3D_Bottom == null)
                                continue;

                            Face2D face2D_Bottom_Flipped = plane_Bottom_Flipped.Convert(face3D_Bottom);
                            face3D_Bottom = plane_Bottom_Flipped.Convert(face2D_Bottom_Flipped);

                            panel = new Panel(construction_Floor, PanelType.Floor, face3D_Bottom);
                            tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(face3D_Bottom.InternalPoint3D(), panel, space));
                        }
                    }

                    double area = face2D.GetArea();
                    double height = elevation_Top - elevation_Bottom;
                    double volume = area * height;
                    
                    Core.ParameterSet parameterSet_Space = new Core.ParameterSet(typeof(Space).Assembly);
                    parameterSet_Space.Add(Query.ParameterName_Area(), area);
                    parameterSet_Space.Add(Query.ParameterName_Volume(), volume);

                    space.Add(parameterSet_Space);
                }
            }

            while (tuples_Point3D.Count > 0)
            {
                Tuple<Point3D, Panel, Space> tuple = tuples_Point3D[0];
                Point3D point3D = tuple.Item1;

                List<Tuple<Point3D, Panel, Space>> tuples_Temp = tuples_Point3D.FindAll(x => point3D.AlmostEquals(x.Item1));
                tuples_Point3D.RemoveAll(x => tuples_Temp.Contains(x));

                Panel panel = tuple.Item2;
                result.AddObject(panel);

                foreach (Tuple<Point3D, Panel, Space> tuple_Temp in tuples_Temp)
                    result.AddRelation(tuple_Temp.Item3, panel);
            }

            result.UpdatePanelTypes(elevation_Ground);
            result.UpdateConstructions();

            return result;
        }
    }
}