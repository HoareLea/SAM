using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<IOpening> Openings(this List<ISAMGeometry3D> sAMGeometry3Ds, OpeningType openingType, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = Geometry.Spatial.Query.Face3Ds(sAMGeometry3Ds, tolerance);
            if (face3Ds == null)
            {
                return null;
            }

            List<IOpening> result = new List<IOpening>();
            foreach(Face3D face3D in face3Ds)
            {
                if (minArea != 0 && face3D.GetArea() < minArea)
                {
                    continue;
                }

                IOpening opening = Opening(openingType, face3D);
                if(opening == null)
                {
                    continue;
                }

                result.Add(opening);
            }

            return result;
        }
    }
}
