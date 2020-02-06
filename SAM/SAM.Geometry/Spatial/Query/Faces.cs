using System;
using System.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face> Faces(this IEnumerable<ISAMGeometry3D> geometry3Ds)
        {
            if (geometry3Ds == null)
                return null;

            List<Face> faces = new List<Face>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Face)
                {
                    faces.Add((Face)geometry3D);
                    continue;
                }

                if (geometry3D is IClosedPlanar3D)
                {
                    faces.Add(new Face((IClosedPlanar3D)geometry3D));
                    continue;
                }

                if(geometry3D is ICurvable3D)
                {
                    List<Point3D> point3Ds = ((ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    faces.Add(new Face(new Polygon3D(point3Ds)));
                }
            }

            return faces;
        }

    }
}
