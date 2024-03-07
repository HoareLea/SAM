using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Planar
{
    public static partial class Create
    {
        public static SAMGeometry2DObjectCollection SAMGeometry2DObjectCollection<T>(this IEnumerable<T> sAMGeometry2Ds, SurfaceAppearance surfaceAppearance, CurveAppearance curveAppearance) where T: ISAMGeometry2D
        {
            if(sAMGeometry2Ds == null)
            {
                return null;
            }

            SAMGeometry2DObjectCollection result = new SAMGeometry2DObjectCollection();
            foreach(T sAMGeometry2D in sAMGeometry2Ds)
            {
                if(sAMGeometry2D is ISAMGeometry2DObject)
                {
                    result.Add((ISAMGeometry2DObject)sAMGeometry2D);
                }
                else if(sAMGeometry2D is Segment2D)
                {
                    result.Add(new Segment2DObject((Segment2D)(object)sAMGeometry2D, curveAppearance));
                }
                else if (sAMGeometry2D is Polyline2D)
                {
                    result.Add(new Polyline2DObject((Polyline2D)(object)sAMGeometry2D, curveAppearance));
                }

                else if (sAMGeometry2D is Polygon2D)
                {
                    result.Add(new Polygon2DObject((Polygon2D)(object)sAMGeometry2D, curveAppearance));
                }

            }

            return result;
        }
    }
}