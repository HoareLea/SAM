using ClipperLib;
using NetTopologySuite.Geometries;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> SimplifyByClipper(this Polygon2D polygon2D, double tolerance = Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            List<IntPoint> intPoints = ((ISegmentable2D)polygon2D).ToClipper(tolerance);
            if (intPoints == null || intPoints.Count == 0)
                return null;

            List<List<IntPoint>> intPointsList = Clipper.SimplifyPolygon(intPoints);
            if (intPointsList == null || intPointsList.Count == 0)
                return null;

            return intPointsList.ConvertAll(x => new Polygon2D(x.ToSAM(tolerance)));
        }

        public static Face2D SimplifyByClipper(this Face2D face2D, double tolerance = Tolerance.MicroDistance)
        {
            //IClosed2D closed2D = face2D.ExternalEdge;
            //if(closed2D is Polygon2D)
            //{
            //    Polygon2D polygon = 
            //}
            
            return face2D;
        }

        public static Polygon SimplifyByClipper(this Polygon polygon, double maxLength, double tolerance = Tolerance.MicroDistance)
        {
            if (polygon == null)
                return null;

            LinearRing linearRing = polygon.ExteriorRing as LinearRing;
            if (linearRing == null)
                return polygon;

            List<IntPoint> intPoints;

            intPoints = linearRing.ToClipper(tolerance);

            intPoints = Clipper.CleanPolygon(intPoints, maxLength);
            if (intPoints == null || intPoints.Count == 0)
                return polygon;

            linearRing = intPoints.ToNTS_LinearRing(tolerance);

            LinearRing[] LinearRings = null;

            LineString[] lineStrings = polygon.InteriorRings;
            if (lineStrings != null && lineStrings.Length > 0)
            {
                List<LinearRing> linearRingsList = new List<LinearRing>();
                foreach (LineString lineString in lineStrings)
                {
                    LinearRing linearRing_Temp = lineString as LinearRing;
                    if (linearRing_Temp == null)
                        continue;

                    intPoints = linearRing_Temp.ToClipper(tolerance);
                    intPoints = Clipper.CleanPolygon(intPoints, maxLength);
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