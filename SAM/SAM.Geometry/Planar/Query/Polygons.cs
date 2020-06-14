using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon> Polygons(this MultiPolygon multiPolygon)
        {
            if (multiPolygon == null)
                return null;

            NetTopologySuite.Geometries.Geometry[] geometries = multiPolygon.Geometries;
            if (geometries == null)
                return null;

            List<Polygon> result = new List<Polygon>();
            foreach(NetTopologySuite.Geometries.Geometry geometry in geometries)
            {
                if(geometry is Polygon)
                {
                    result.Add((Polygon)geometry);
                }
                else if(geometry is MultiPolygon)
                {
                    List<Polygon> polygons = Polygons((MultiPolygon)geometry);
                    if (polygons != null && polygons.Count > 0)
                        result.AddRange(polygons);
                }
                else if(geometry is LinearRing)
                {
                    result.Add(new Polygon((LinearRing)geometry));
                }

            }

            return result;
        }
    }
}