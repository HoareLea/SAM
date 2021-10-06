using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static List<HostBuildingElement> MergeCoplanar(this IEnumerable<HostBuildingElement> hostBuildingElements, double offset, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (hostBuildingElements == null)
                return null;
            
            List<HostBuildingElement> redundantHostBuildingElements = new List<HostBuildingElement>();

            return MergeCoplanar(hostBuildingElements?.ToList(), offset, ref redundantHostBuildingElements, validateConstruction, minArea, tolerance);
        }

        private static List<HostBuildingElement> MergeCoplanar(this List<HostBuildingElement> hostBuildingElements, double offset, ref List<HostBuildingElement> redundantHostBuildingElements, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (hostBuildingElements == null)
                return null;

            List<HostBuildingElement> hostBuildingElements_Temp = hostBuildingElements.ToList();

            hostBuildingElements_Temp.Sort((x, y) => y.Face3D.GetArea().CompareTo(x.Face3D.GetArea()));

            List<HostBuildingElement> result = new List<HostBuildingElement>(hostBuildingElements);
            HashSet<Guid> guids = new HashSet<Guid>();

            while (hostBuildingElements_Temp.Count > 0)
            {
                HostBuildingElement hostBuildingElement = hostBuildingElements_Temp[0];
                hostBuildingElements_Temp.RemoveAt(0);

                Plane plane = hostBuildingElement.Face3D.GetPlane();
                if (plane == null)
                    continue;

                List<HostBuildingElement> hostBuildingElement_Offset = new List<HostBuildingElement>();
                foreach (HostBuildingElement hostBuildingElement_Temp in hostBuildingElements_Temp)
                {
                    if(validateConstruction)
                    {
                        BuildingElementType construction_Temp = hostBuildingElement_Temp.SAMType as BuildingElementType;
                        BuildingElementType construction = hostBuildingElement.SAMType as BuildingElementType;

                        if (construction_Temp != null && construction != null)
                        {
                            if (!construction_Temp.Name.Equals(construction.Name))
                                continue;
                        }
                        else if (!(construction_Temp == null && construction == null))
                        {
                            continue;
                        }
                    }

                    Plane plane_Temp = hostBuildingElement_Temp?.Face3D.GetPlane();
                    if (plane == null)
                        continue;

                    if (!plane.Coplanar(plane_Temp, tolerance))
                        continue;

                    double distance = plane.Distance(plane_Temp, tolerance);
                    if (distance > offset)
                        continue;

                    hostBuildingElement_Offset.Add(hostBuildingElement_Temp);
                }

                if (hostBuildingElement_Offset == null || hostBuildingElement_Offset.Count == 0)
                    continue;

                hostBuildingElement_Offset.Add(hostBuildingElement);

                List<Tuple<Polygon, HostBuildingElement>> tuples_Polygon = new List<Tuple<Polygon, HostBuildingElement>>();
                List<Point2D> point2Ds = new List<Point2D>(); //Snap Points
                foreach (HostBuildingElement hostBuildingElement_Temp in hostBuildingElement_Offset)
                {
                    Face3D face3D = hostBuildingElement_Temp.Face3D;
                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => Geometry.Planar.Modify.Add(point2Ds, plane.Convert(x), tolerance));
                    }

                    Face2D face2D = plane.Convert(plane.Project(face3D));

                    //tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(), panel_Temp));
                    tuples_Polygon.Add(new Tuple<Polygon, HostBuildingElement>(face2D.ToNTS(tolerance), hostBuildingElement_Temp));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                polygons_Temp = Geometry.Planar.Query.Union(polygons_Temp);
                foreach (Polygon polygon in polygons_Temp)
                {
                    if (polygon.Area < minArea)
                        continue;

                    List<Tuple<Polygon, HostBuildingElement>> tuples_HostBuildingElement = tuples_Polygon.FindAll(x => polygon.Contains(x.Item1.InteriorPoint));
                    if (tuples_HostBuildingElement == null || tuples_HostBuildingElement.Count == 0)
                        continue;

                    tuples_HostBuildingElement.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    foreach (Tuple<Polygon, HostBuildingElement> tuple in tuples_HostBuildingElement)
                    {
                        result.Remove(tuple.Item2);
                        hostBuildingElements_Temp.Remove(tuple.Item2);
                    }

                    HostBuildingElement hostBuildingElement_Old = tuples_HostBuildingElement.First().Item2;
                    tuples_HostBuildingElement.RemoveAt(0);
                    redundantHostBuildingElements?.AddRange(tuples_HostBuildingElement.ConvertAll(x => x.Item2));

                    if (hostBuildingElement_Old == null)
                        continue;

                    Polygon polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_Snapper(polygon, tolerance);
                    polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_TopologyPreservingSimplifier(polygon_Temp, tolerance);

                    Face2D face2D = polygon_Temp.ToSAM(minArea, Core.Tolerance.MicroDistance)?.Snap(point2Ds, tolerance);
                    if (face2D == null)
                        continue;

                    Face3D face3D = new Face3D(plane, face2D);
                    Guid guid = hostBuildingElement_Old.Guid;
                    if (guids.Contains(guid))
                        guid = Guid.NewGuid();

                    //Adding Openings from redundant Panels
                    List<Opening> openings = new List<Opening>();
                    if (redundantHostBuildingElements != null && redundantHostBuildingElements.Count != 0)
                    {
                        foreach (HostBuildingElement hostBuildingElement_redundant in redundantHostBuildingElements)
                        {
                            if (hostBuildingElement_redundant == null)
                                continue;

                            List<Opening> openings_Temp = hostBuildingElement_redundant.Openings;
                            if (openings_Temp == null || openings_Temp.Count == 0)
                                continue;

                            openings.AddRange(openings_Temp);
                        }
                    }

                    HostBuildingElement hostBuildingElement_New = Create.HostBuildingElement(guid, face3D, hostBuildingElement_Old.SAMType as HostBuildingElementType, tolerance);

                    result.Add(hostBuildingElement_New);
                }
            }

            return result;
        }
    }
}