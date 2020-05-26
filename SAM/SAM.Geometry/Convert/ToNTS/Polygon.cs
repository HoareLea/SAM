using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polygon ToNTS(this Face face, double tolerance = Core.Tolerance.MicroDistance)
        {
            LinearRing linearRing_ExternalEdge = face?.ExternalEdge?.ToNTS(tolerance);
            if (linearRing_ExternalEdge == null)
                return null;

            List<LinearRing> linearRingsList_InternalEdges = face.InternalEdges?.ConvertAll(x => x.ToNTS(tolerance));

            LinearRing[] linearRingsArray_InternalEdges = null;
            if (linearRingsList_InternalEdges != null && linearRingsList_InternalEdges.Count > 0)
                linearRingsArray_InternalEdges = linearRingsList_InternalEdges.ToArray();

            if (linearRingsArray_InternalEdges == null)
                return new Polygon(linearRing_ExternalEdge);
            else
                return new Polygon(linearRing_ExternalEdge, linearRingsArray_InternalEdges);
        }

        public static Polygon ToNTS(this Face2D face2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            return ToNTS(face2D as Face, tolerance);
        }

        public static Polygon ToNTS_Polygon(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            return new Polygon((polygon2D as IClosed2D)?.ToNTS(tolerance));
        }
    }
}