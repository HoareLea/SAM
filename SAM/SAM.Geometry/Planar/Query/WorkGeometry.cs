using NetTopologySuite.Geometries;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static NetTopologySuite.Geometries.Geometry WorkGeometry(NetTopologySuite.Geometries.Geometry geometry, double tolerance = Core.Tolerance.MacroDistance)
        {
            if(geometry == null)
            {
                return null;
            }

            Envelope envelope = geometry.Boundary.EnvelopeInternal;

            double buffer = 0.5 * envelope.MaxExtent;

            envelope = new Envelope(envelope.MinX - buffer, envelope.MaxX + buffer, envelope.MinY - buffer, envelope.MaxY + buffer);
            var bounds = geometry.Factory.ToGeometry(envelope);

            return geometry.Factory.CreateGeometryCollection(new[] { geometry, bounds });
        }
    }
}