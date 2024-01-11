using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static List<Polyline2D> ToSAM(this MultiLineString multiLineString, double tolerance = Core.Tolerance.Distance)
        {
            IEnumerable<NetTopologySuite.Geometries.Geometry> geometries = multiLineString?.Geometries;
            if (geometries == null)
                return null;

            List<Polyline2D> result = new List<Polyline2D>();
            foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
            {
                LineString lineString = geometry_Temp as LineString;
                if (lineString == null)
                    continue;

                result.Add(lineString.ToSAM(tolerance));
            }
            return result;
        }
    }
}