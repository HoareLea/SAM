using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public interface ISegmentable3D : ICurvable3D
    {
        List<Segment3D> GetSegments();

        List<Point3D> GetPoints();

        bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance);
    }
}