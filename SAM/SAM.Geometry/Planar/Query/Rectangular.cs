namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Rectangular(this IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            return Rectangular(closed2D, out Rectangle2D rectangle2D, tolerance);
        }

        public static bool Rectangular(this IClosed2D closed2D, out Rectangle2D rectangle2D, double tolerance = Core.Tolerance.Distance)
        {
            rectangle2D = null;

            if (closed2D == null)
                return false;

            if (closed2D is Circle2D)
                return false;

            if (closed2D is Triangle2D)
                return false;

            if (closed2D is Rectangle2D)
            {
                rectangle2D = new Rectangle2D((Rectangle2D)closed2D);
                return true;
            }

            if (closed2D is BoundingBox2D)
            {
                rectangle2D = new Rectangle2D((BoundingBox2D)closed2D);
                return true;
            }

            if (closed2D is ISegmentable2D)
            {
                rectangle2D = Create.Rectangle2D(((ISegmentable2D)closed2D).GetPoints());

                bool result = System.Math.Abs(closed2D.GetArea() - rectangle2D.GetArea()) < tolerance;
                if (!result)
                    rectangle2D = null;

                return result;
            }

            throw new System.NotImplementedException();
        }
    }
}