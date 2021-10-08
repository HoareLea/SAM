using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static Face3D Face3D(this HostPartition hostPartition, bool cutOpenings, double tolerance = Core.Tolerance.Distance)
        {
            Face3D result = hostPartition?.Face3D;

            if (result == null || !cutOpenings)
            {
                return result;
            }

            List<Opening> openings = hostPartition.Openings;
            if(openings == null || openings.Count == 0)
            {
                return result;
            }

            Plane plane = result.GetPlane();

            Face2D face2D = plane?.Convert(result);
            if (face2D == null)
                return null;

            foreach(Opening opening in openings)
            {
                Face3D face3D_Aperture = opening?.Face3D;
                if (face3D_Aperture == null)
                    continue;

                Face2D face2D_Aperture = plane.Convert(face3D_Aperture);

                List<Face2D> face2Ds = face2D.Difference(face2D_Aperture, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                face2D = face2Ds[0];
            }

            return plane.Convert(face2D);

        }
    }
}