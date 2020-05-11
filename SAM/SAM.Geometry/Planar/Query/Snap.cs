﻿using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> Snap(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            LinearRing linearRing_1 = (polygon2D_1 as IClosed2D)?.ToNTS(tolerance);
            if (linearRing_1 == null)
                return null;

            LinearRing linearRing_2 = (polygon2D_2 as IClosed2D)?.ToNTS(tolerance);
            if (linearRing_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = GeometrySnapper.Snap(linearRing_1, linearRing_2, snapDistance);
            if (geometries == null)
                return null;

            return geometries.ToList().ConvertAll(x => (x as LinearRing).ToSAM(tolerance));
        }

        public static List<Face2D> Snap(this Face2D face2D_1, Face2D face2D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Polygon polygon_1 = face2D_1?.ToNTS(tolerance);
            if (polygon_1 == null)
                return null;

            Polygon polygon_2 = face2D_2?.ToNTS(tolerance);
            if (polygon_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = GeometrySnapper.Snap(polygon_1, polygon_2, snapDistance);
            if (geometries == null)
                return null;

            return geometries.ToList().ConvertAll(x => (x as Polygon).ToSAM(tolerance));
        }
    }
}