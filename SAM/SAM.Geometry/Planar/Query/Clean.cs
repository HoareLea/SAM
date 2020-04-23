using System.Collections.Generic;

using ClipperLib;

using NetTopologySuite.Geometries;


namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon Clean(this Polygon polygon, double distance, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon == null)
                return null;

            LinearRing linearRing = polygon.ExteriorRing as LinearRing;
            if (linearRing == null)
                return polygon;

            List<IntPoint> intPoints;

            intPoints =  linearRing.ToClipper(tolerance);

            intPoints = Clipper.CleanPolygon(intPoints, distance);
            if (intPoints == null || intPoints.Count == 0)
                return polygon;

            linearRing = intPoints.ToNTS_LinearRing(tolerance);

            LinearRing[] LinearRings = null;

            LineString[] lineStrings = polygon.InteriorRings;
            if(lineStrings != null && lineStrings.Length > 0 )
            {
                List<LinearRing> linearRingsList = new List<LinearRing>();
                foreach(LineString lineString in lineStrings)
                {
                    LinearRing linearRing_Temp = lineString as LinearRing;
                    if (linearRing_Temp == null)
                        continue;

                    intPoints = linearRing_Temp.ToClipper(tolerance);
                    intPoints = Clipper.CleanPolygon(intPoints, distance);
                    linearRing_Temp = intPoints.ToNTS_LinearRing(tolerance);
                    linearRingsList.Add(linearRing_Temp);
                }

                LinearRings = linearRingsList.ToArray();
            }

            if (LinearRings == null || LinearRings.Length == 0)
                return new Polygon(linearRing);
            else
                return new Polygon(linearRing, LinearRings);
        }
    }
}
