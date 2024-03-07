using SAM.Geometry.Object.Planar;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object
{
    public static partial class Convert
    {
        public static List<ISAMGeometry> ToSAM_ISAMGeometry(this ISAMGeometryObject sAMGeometryObject)
        {
            if (sAMGeometryObject == null)
            {
                return null;
            }

            if (sAMGeometryObject is IPolyline2DObject)
            {
                return new List<ISAMGeometry>() { ((IPolyline2DObject)sAMGeometryObject).Polyline2D };
            }

            if (sAMGeometryObject is ISegment2DObject)
            {
                return new List<ISAMGeometry>() { ((ISegment2DObject)sAMGeometryObject).Segment2D };
            }

            if (sAMGeometryObject is IPolygon2DObject)
            {
                return new List<ISAMGeometry>() { ((IPolygon2DObject)sAMGeometryObject).Polygon2D };
            }

            if (sAMGeometryObject is IEnumerable<ISAMGeometry2DObject>)
            {
                List<ISAMGeometry> result = new List<ISAMGeometry>();

                foreach (ISAMGeometry2DObject sAMGeometry2DObject in (IEnumerable<ISAMGeometry2DObject>)sAMGeometryObject)
                {
                    List<ISAMGeometry> geometries = ToSAM_ISAMGeometry(sAMGeometry2DObject);
                    if(geometries != null)
                    {
                        result.AddRange(geometries);
                    }
                }

                return result;
            }

            return null;
        }
    }
}
