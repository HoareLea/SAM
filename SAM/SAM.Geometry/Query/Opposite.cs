namespace SAM.Geometry
{
    public  static partial class Query
    {
        public static Alignment Opposite(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Horizontal:
                    return Alignment.Vertical;
                case Alignment.Vertical:
                    return Alignment.Horizontal;
                default:
                    return Alignment.Undefined;
            }
        }

        public static Orientation Opposite(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Clockwise:
                    return Orientation.CounterClockwise;
                case Orientation.CounterClockwise:
                    return Orientation.Clockwise;
                default:
                    return Orientation.Undefined;
            }
        }
    }
}
