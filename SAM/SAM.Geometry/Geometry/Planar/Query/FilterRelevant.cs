using NetTopologySuite.Geometries.Prepared;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static NetTopologySuite.Geometries.Geometry FilterRelevant(NetTopologySuite.Geometries.Geometry geometry, IEnumerable<NetTopologySuite.Geometries.Geometry> geometries)
        {
            if(geometry == null)
            {
                return null;
            }

            if(geometries == null || geometries.Count() == 0)
            {
                return geometry;
            }

            List<NetTopologySuite.Geometries.Geometry> geometries_Temp = new List<NetTopologySuite.Geometries.Geometry>();
            
            IPreparedGeometry preparedGeometry = PreparedGeometryFactory.Prepare(geometry);
            
            foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
            {
                if (preparedGeometry.Contains(geometry_Temp))
                {
                    geometries_Temp.Add(geometry_Temp);
                }
            }
                
            return geometry.Factory.BuildGeometry(geometries_Temp);
        }
    }
}