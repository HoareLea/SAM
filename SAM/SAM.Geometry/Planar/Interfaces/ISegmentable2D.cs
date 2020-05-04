using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public interface ISegmentable2D : ICurvable2D
    {
        List<Segment2D> GetSegments();

        List<Point2D> GetPoints();

        double Distance(ISegmentable2D segmentable2D);

        double Distance(Point2D point2D);

        bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance);

        double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance);

        Point2D GetPoint(double parameter, bool inverted = false);

        ISegmentable2D Trim(double parameter, bool inverted = false);

        double GetLength();
    }
}