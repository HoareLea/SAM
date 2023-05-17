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
            envelope = new Envelope(envelope.MinX - 1, envelope.MaxX + 1, envelope.MinY - 1, envelope.MaxY + 1);
            var bounds = geometry.Factory.ToGeometry(envelope);

            return geometry.Factory.CreateGeometryCollection(new[] { geometry, bounds });
        }
    }
}