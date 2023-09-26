using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public class Solver2D
    {
        private List<Solver2DData> solver2DDatas;
        private List<IClosed2D> obstacles2D;
        private Rectangle2D area;

        public Solver2D(Rectangle2D area, List<IClosed2D> obstacles2D)
        {
            this.area = area;
            this.obstacles2D = obstacles2D;
        }
         
        public bool Add(Rectangle2D rectangle2D, Polyline2D polyline2D, int priority = int.MinValue, object tag = null)
        {
            if (rectangle2D == null || polyline2D == null)
            {
                return false;
            }

            return Add((IClosed2D)rectangle2D, (ISAMGeometry2D)polyline2D, priority, tag);
        }

        public bool Add(Rectangle2D rectangle2D, Point2D point2D, int priority = int.MinValue, object tag = null)
        { 
            if (rectangle2D == null || point2D == null)
            {
                return false;
            }

            return Add((IClosed2D)rectangle2D, (ISAMGeometry2D)point2D, priority, tag);
        }

        private bool Add(IClosed2D closed2D, ISAMGeometry2D geometry2D, int priority = int.MinValue, object tag = null)
        {
            if (closed2D == null || geometry2D == null)
            {
                return false;
            }

            if (solver2DDatas == null)
            {
                solver2DDatas = new List<Solver2DData>();
            }

            Solver2DData solver2DData = new Solver2DData(closed2D, geometry2D);
            solver2DData.Priority = priority;
            solver2DData.Tag = tag;

            solver2DDatas.Add(solver2DData);
            return true;
        }

        public List<Solver2DResult> Solve(Solver2DSettings solver2DSettings)
        {
            if (solver2DDatas == null || solver2DDatas.Count == 0)
            {
                return null;
            }

            // Apply priority order
            solver2DDatas.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            List<Solver2DResult> result = new List<Solver2DResult>();

            foreach (Solver2DData solver2DData in solver2DDatas)
            {
                Rectangle2D rectangle2D = solver2DData.Closed2D<Rectangle2D>();
                if (rectangle2D == null)
                {
                    throw new System.NotImplementedException();
                }
                Rectangle2D rectangle2D_New = null;
                Point2D center = rectangle2D.GetCentroid();

                ISAMGeometry2D sAMGeometry2D = solver2DData.Geometry2D<ISAMGeometry2D>();
                if (sAMGeometry2D is Point2D)
                {
                    Point2D point2D = (Point2D)sAMGeometry2D;
                    double defaultDistanceFromPoint = point2D.Distance(center);
                    List<Vector2D> offsets = generateOffsets(defaultDistanceFromPoint);

                    Rectangle2D rectangleWithGivenPointInCenter = rectangle2D.GetMoved(new Vector2D(center, point2D));

                    for (int i = 0; i < solver2DSettings.MaxStepPoint; i++)
                    {
                        if (rectangle2D_New != null) break;

                        foreach (Vector2D offset in offsets)
                        {
                            Vector2D scaledOffset = offset * (1.0 + ((double)i / 5.0)); //solver2DSettings.MoveDistancePoint);
                            Rectangle2D rectangle_Temp = rectangleWithGivenPointInCenter.GetMoved(scaledOffset);

                            if (Query.Inside(area, rectangle_Temp) && !intersect(rectangle_Temp, result))
                            {
                                rectangle2D_New = rectangle_Temp;
                                break;
                            }
                        }
                    }
                }
                else if (sAMGeometry2D is Polyline2D)
                {
                    Polyline2D polyline2D = (Polyline2D)sAMGeometry2D;
                    List<Segment2D> segment2Ds = polyline2D.GetSegments();

                    Point2D point = polyline2D.Closest(center);
                    double distanceToCenter = point.Distance(center);

                    for (int i = 0; i < solver2DSettings.MaxStepPolyline; i++)
                    {
                        if (rectangle2D_New != null) break;

                        for (int j = -1; j <= 1; j += 2)
                        {
                            double xNew = point.X + i * j * solver2DSettings.MoveDistancePolyLine;
                            double yNew = getY(polyline2D, xNew);
                            if (yNew == double.NaN) continue;
                            Point2D newPoint = new Point2D(xNew, yNew);

                            List<Segment2D> segments = polyline2D.ClosestSegment2Ds(newPoint);
                            if (segments == null) continue;

                            Segment2D segment = segments[0];
                            bool clockwise = segment.Direction.GetPerpendicular().Y < 0;
                            Rectangle2D rectangle_Temp = fix(Query.MoveToSegment2D(rectangle2D, segment, newPoint, distanceToCenter, clockwise), rectangle2D);

                            if (Query.Inside(area, rectangle_Temp) && !intersect(rectangle_Temp, result))
                            {
                                rectangle2D_New = rectangle_Temp;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }

                result.Add(new Solver2DResult(solver2DData, rectangle2D_New));
            }

            return result;
        }

        private double getY(Polyline2D polyLine2D, double x)
        {
            List<Segment2D> polyLine2DSegments = polyLine2D.Segment2Ds();
            Segment2D resultSegment = null;

            foreach (Segment2D segment in polyLine2DSegments)
            {
                if (segment.Min.X <= x && x <= segment.Max.X)
                {
                    resultSegment = segment;
                    break;
                }
            }
            if (resultSegment == null) return double.NaN;

            List<Point2D> points = resultSegment.GetPoints();
            if (points == null || points.Count < 2) return double.NaN;

            Math.LinearEquation linearEquation = Math.Create.LinearEquation(points[0].X, points[0].Y, points[1].X, points[1].Y);
            if (linearEquation == null) return double.NaN;

            return linearEquation.Evaluate(x);
        }
        private List<Vector2D> generateOffsets(double distance)
        {

            List<Vector2D> offsets = new List<Vector2D>();
            double offsetAngle = 180;

            for (double angle = 0; angle < 360; angle += offsetAngle)
            {
                double radians = System.Math.PI * angle / 180;
                double offsetX = distance * System.Math.Sin(radians);
                double offsetY = distance * System.Math.Cos(radians);

                offsets.Add(new Vector2D(offsetX, offsetY));
            }

            offsetAngle = 90;

            for (double angle = 0; angle < 360; angle += offsetAngle)
            {
                double radians = System.Math.PI * angle / 180;
                double offsetX = distance * System.Math.Sin(radians);
                double offsetY = distance * System.Math.Cos(radians);

                offsets.Add(new Vector2D(offsetX, offsetY));
            }

            for (double angle = 45; angle < 360; angle += offsetAngle)
            {
                double radians = System.Math.PI * angle / 180; ;
                double offsetX = distance * System.Math.Sin(radians);
                double offsetY = distance * System.Math.Cos(radians);

                offsets.Add(new Vector2D(offsetX, offsetY));
            }
            return offsets;
        }

        private Rectangle2D fix(Rectangle2D calculatedRectangle, Rectangle2D defaultRectangle)
        {
            if (calculatedRectangle == null || defaultRectangle == null)
            {
                return calculatedRectangle;
            }
            if (System.Math.Abs(defaultRectangle.Width - calculatedRectangle.Width) < SAM.Core.Tolerance.MacroDistance)
            {
                return calculatedRectangle;
            }

            Rectangle2D result = new Rectangle2D(calculatedRectangle.Origin, -calculatedRectangle.Height, calculatedRectangle.Width, calculatedRectangle.WidthDirection);
            return result;
        }   

        private bool intersect(Rectangle2D rectangle2D, List<Solver2DResult> solver2DResults)
        {
            return (obstacles2D.Find(x => x.InRange(rectangle2D) == true) != null) ||
                    (solver2DResults.Find(x => x.Closed2D<Rectangle2D>().InRange(rectangle2D) == true) != null) ||
                    (solver2DResults.Find(x => rectangle2D.InRange(x.Closed2D<Rectangle2D>()) == true) != null);
        }
    }
}
