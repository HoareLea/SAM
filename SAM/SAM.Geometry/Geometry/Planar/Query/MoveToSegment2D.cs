using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Method returns rectangle of size specified by input
        /// at a distance from point on a line
        /// </summary>
        /// <param name="rectangle2D">Rectangle whose result has a shape</param>
        /// <param name="segment2D">Line along which the rectangle will be formed </param>
        /// <param name="point2D">Point on a segment2D</param>
        /// <param name="distanceToCenter">Distance from center of result rectangle to point on a segment2D</param>
        /// <param name="clockwise">Determines whether rectangle has to be on or under line</param>
        /// <returns></returns>
        public static Rectangle2D MoveToSegment2D(this Rectangle2D rectangle2D, Segment2D segment2D, Point2D point2D, double distanceToCenter, bool clockwise = true)
        {
            if (rectangle2D == null || segment2D == null || point2D == null)
            {
                return rectangle2D;
            }

            Vector2D horizontalVector = segment2D.Direction.Unit;
            Vector2D verticalVector = horizontalVector.GetPerpendicular();

            Point2D rectangleCenter = point2D.GetMoved(verticalVector * distanceToCenter);

            if (clockwise)
            {
                rectangleCenter = point2D.GetMoved(verticalVector.GetNegated() * distanceToCenter);
            }

            Vector2D widthVector = horizontalVector * (rectangle2D.Width / 2);
            Vector2D heightVector = verticalVector * (rectangle2D.Height / 2);

            List<Point2D> rectangleCornerPoints = new List<Point2D>()
            {
                rectangleCenter.GetMoved(widthVector.GetNegated() + heightVector),
                rectangleCenter.GetMoved(widthVector + heightVector),
                rectangleCenter.GetMoved(widthVector + heightVector.GetNegated()),
                rectangleCenter.GetMoved(widthVector.GetNegated() + heightVector.GetNegated())
            };


            return Create.Rectangle2D(rectangleCornerPoints);
        }
    }
}