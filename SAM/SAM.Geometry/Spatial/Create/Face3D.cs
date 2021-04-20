using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Face3D Face3D(this Segment3D segment3D, double height, double tolerance = Core.Tolerance.Angle)
        {
            if (segment3D == null || height == 0)
                return null;

            Polygon3D polygon3D = Polygon3D(segment3D, height);
            if (polygon3D == null)
                return null;

            return new Face3D(polygon3D);
        }
    }
}