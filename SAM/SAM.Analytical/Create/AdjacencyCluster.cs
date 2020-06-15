using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double tolerance = Core.Tolerance.Distance)
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
                    foreach(Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                        Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });
                        Panel panel = new Panel(construction_Wall, PanelType.Wall, new Face3D(polygon3D));

                        tuples.Add(new Tuple<Segment2D, Panel, Space> (segment2D, panel, space));
                    }

                    adjacencyCluster.AddObject(space);

                    Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                    polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));

                    Panel panel_Min = new Panel(construction_Floor, PanelType.Floor, new Face3D(polygon3D_Min));
                    adjacencyCluster.AddObject(panel_Min);
                    adjacencyCluster.AddRelation(space, panel_Min);

                    Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                    Panel panel_Max = new Panel(construction_Roof, PanelType.Roof, new Face3D(polygon3D_Max));
                    adjacencyCluster.AddObject(panel_Max);
                    adjacencyCluster.AddRelation(space, panel_Max);
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

            adjacencyCluster.UpdatePanelType();

            return adjacencyCluster;
        }
    }
}