using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Wall> Walls(this IEnumerable<ISegmentable3D> segmentable3Ds, double height, WallType wallType = null)
        {
            if (segmentable3Ds == null)
                return null;

            List<Wall> result = new List<Wall>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                {
                    continue;
                }

                foreach (Segment3D segment3D in segment3Ds)
                {
                    Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(segment3D, height);
                    if (polygon3D == null)
                    {
                        return null;
                    }

                    WallType wallType_Temp = wallType;
                    if (wallType_Temp == null)
                    {
                        wallType_Temp = new WallType(string.Empty);
                    }

                    result.Add(new Wall(wallType_Temp, new Face3D(polygon3D)));
                }
            }

            return result;
        }
    }
}