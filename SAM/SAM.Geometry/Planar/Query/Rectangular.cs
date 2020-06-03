namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Rectangular(this IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null)
                return false;

            if (closed2D is Rectangle2D)
                return true;

            if (closed2D is BoundingBox2D)
                return true;

            if (closed2D is Circle2D)
                return false;

            if (closed2D is Triangle2D)
                return false;

            if (closed2D is ISegmentable2D)
            {
                Rectangle2D rectangle2D = Create.Rectangle2D(((ISegmentable2D)closed2D).GetPoints());

                return System.Math.Abs(closed2D.GetArea() - rectangle2D.GetArea()) < tolerance;
            }

            throw new System.NotImplementedException();
        }
    }
}