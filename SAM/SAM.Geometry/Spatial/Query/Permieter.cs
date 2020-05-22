using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Permieter(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return double.NaN;

            if(closedPlanar3D is ISegmentable3D)
            {
                List<Segment3D> segment3Ds = ((ISegmentable3D)closedPlanar3D).GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                    return double.NaN;

                double perimeter = 0;
                segment3Ds.ForEach(x => perimeter += x.GetLength());
                return perimeter;

            }

            throw new System.NotImplementedException();
        }
    }
}