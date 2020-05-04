namespace SAM.Geometry
{
    public static partial class Query
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

        public static Corner Opposite(Corner corner, Alignment alignment = Alignment.Undefined)
        {
            switch (alignment)
            {
                case Alignment.Horizontal:
                    switch (corner)
                    {
                        case Corner.BottomLeft:
                            return Corner.TopLeft;

                        case Corner.BottomRight:
                            return Corner.TopRight;

                        case Corner.TopLeft:
                            return Corner.BottomLeft;

                        case Corner.TopRight:
                            return Corner.BottomRight;
                    }
                    break;

                case Alignment.Undefined:
                    switch (corner)
                    {
                        case Corner.BottomLeft:
                            return Corner.TopRight;

                        case Corner.BottomRight:
                            return Corner.TopLeft;

                        case Corner.TopLeft:
                            return Corner.BottomRight;

                        case Corner.TopRight:
                            return Corner.BottomLeft;
                    }
                    break;

                case Alignment.Vertical:
                    switch (corner)
                    {
                        case Corner.BottomLeft:
                            return Corner.BottomRight;

                        case Corner.BottomRight:
                            return Corner.BottomLeft;

                        case Corner.TopLeft:
                            return Corner.TopRight;

                        case Corner.TopRight:
                            return Corner.TopLeft;
                    }
                    break;
            }

            return Corner.Undefined;
        }
    }
}