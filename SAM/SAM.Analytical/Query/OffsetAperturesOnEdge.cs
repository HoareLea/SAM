using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Aperture> OffsetAperturesOnEdge(this Panel panel, double distance, double tolerance = Core.Tolerance.Distance)
        {
            List<Aperture> apertures = panel?.Apertures;
            if (apertures == null || apertures.Count == 0)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            Plane plane = face3D.GetPlane();

            Polygon2D externalEdge = face3D.ExternalEdge2D as Polygon2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Polygon2D> polygon2Ds = Geometry.Planar.Query.Offset(externalEdge, -distance, tolerance);
            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            externalEdge = polygon2Ds.Last();

            for (int i = 0; i < apertures.Count; i++)
            {
                Aperture aperture = apertures[i];

                IClosedPlanar3D closedPlanar3D = aperture?.GetFace3D()?.GetExternalEdge3D();
                if (closedPlanar3D == null)
                    continue;

                closedPlanar3D = plane.Project(closedPlanar3D);

                Polygon2D externalEdge_Aperture = plane.Convert(closedPlanar3D) as Polygon2D;
                if (externalEdge_Aperture == null)
                    continue;

                List<Point2D> point2Ds_Intersections = Geometry.Planar.Query.Intersections(externalEdge, externalEdge_Aperture);
                if (point2Ds_Intersections == null || point2Ds_Intersections.Count == 0)
                    if(!externalEdge_Aperture.Inside(externalEdge))
                    continue;                    

                polygon2Ds = Geometry.Planar.Query.Intersection(externalEdge_Aperture, externalEdge, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

                closedPlanar3D = plane.Convert(polygon2Ds.Last());

                apertures[i] = new Aperture(aperture.ApertureConstruction, closedPlanar3D, aperture.PlanarBoundary3D.GetFace3D().GetPlane().Origin);
            }

            return apertures;
        }
    }
}