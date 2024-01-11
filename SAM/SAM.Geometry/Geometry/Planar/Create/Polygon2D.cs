namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Polygon2D Polygon2D(this IClosed2D closed2D)
        {
            if(closed2D == null)
            {
                return null;
            }

            if(closed2D is ISegmentable2D)
            {
                return new Polygon2D(((ISegmentable2D)closed2D).GetPoints());
            }

            return null;
        }
    }
}