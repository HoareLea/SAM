using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<IOpening> OffsetAperturesOnEdge(this BuildingModel buildingModel, double distance, double tolerance = Core.Tolerance.Distance)
        {
            if (buildingModel == null)
            {
                return null;
            }

            List<IHostPartition> hostPartitions = buildingModel.GetPartitions<IHostPartition>();
            if (hostPartitions == null || hostPartitions.Count == 0)
            {
                return null;
            }

            List<IOpening> result = new List<IOpening>();
            foreach(IHostPartition hostPartition in hostPartitions)
            {
                List<IOpening> openings = hostPartition?.GetOpenings();
                if(openings != null && openings.Count != 0)
                {
                    List<IOpening> openings_Offset = hostPartition.OffsetAperturesOnEdge(distance, tolerance);
                    if(openings_Offset != null && openings_Offset.Count != 0)
                    {
                        buildingModel.Add(hostPartition);
                        result.AddRange(openings_Offset);
                    }
                }
            }

            return result;
        }

        public static List<IOpening> OffsetAperturesOnEdge(this IHostPartition hostPartition, double distance, double tolerance = Core.Tolerance.Distance)
        {
            List<IOpening> openings = hostPartition?.GetOpenings();
            if (openings == null || openings.Count == 0)
            {
                return null;
            }

            Face3D face3D = hostPartition.Face3D;
            if (face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();

            Polygon2D externalEdge = face3D.ExternalEdge2D as Polygon2D;
            if (externalEdge == null)
                throw new System.NotImplementedException();

            List<Polygon2D> polygon2Ds = Geometry.Planar.Query.Offset(externalEdge, -distance, tolerance);
            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            externalEdge = polygon2Ds.Last();

            List<IOpening> result = new List<IOpening>();
            for (int i = 0; i < openings.Count; i++)
            {
                IOpening opening = openings[i];

                IClosedPlanar3D closedPlanar3D = opening?.Face3D?.GetExternalEdge3D();
                if (closedPlanar3D == null)
                {
                    continue;
                }

                closedPlanar3D = plane.Project(closedPlanar3D);

                Polygon2D externalEdge_Aperture = plane.Convert(closedPlanar3D) as Polygon2D;
                if (externalEdge_Aperture == null)
                {
                    continue;
                }

                List<Point2D> point2Ds_Intersections = Geometry.Planar.Query.Intersections(externalEdge, externalEdge_Aperture);
                if (point2Ds_Intersections == null || point2Ds_Intersections.Count == 0)
                {
                    if (!externalEdge_Aperture.Inside(externalEdge))
                    {
                        continue;
                    }
                }

                polygon2Ds = Geometry.Planar.Query.Intersection(externalEdge_Aperture, externalEdge, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                {
                    continue;
                }

                polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

                closedPlanar3D = plane.Convert(polygon2Ds.Last());

                IOpening opening_New = Create.Opening(opening.Guid, opening, new Face3D(closedPlanar3D), opening.Face3D.GetPlane().Origin);
                if(opening_New == null)
                {
                    continue;
                }

                List<IOpening> openings_New = hostPartition.AddOpening(opening_New, tolerance);
                if(openings_New != null)
                {
                    result.AddRange(openings_New);
                }
            }

            return result;
        }
    }
}