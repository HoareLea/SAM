using System.Collections.Generic;
using System.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> SnapByOffset(this IEnumerable<Panel> panels, double offset = 0.2, double maxDixtance = double.NaN)
        {
            List<Point3D> point3DList = Point3D.Generate(new BoundingBox3D(panels.ToList().ConvertAll(x => x.GetBoundingBox(offset))), offset);

            return SnapByPoints(panels, point3DList, maxDixtance);
        }
    }
}
