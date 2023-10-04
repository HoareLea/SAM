using System;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Segment2D Segment2D(this Line2D line2D, Rectangle2D rectangle2D)
        {
            if(line2D == null || rectangle2D == null)
            {
                return null;
            }

            throw new NotImplementedException();
        }

    
    }
}