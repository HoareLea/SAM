using System;
using System.Linq;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Query
    {
        public static List<Spatial.Face> Faces(this IEnumerable<Spatial.IGeometry3D> geometry3Ds)
        {
            if (geometry3Ds == null)
                return null;

            List<Spatial.Face> faces = new List<Spatial.Face>();
            foreach (Spatial.IGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Spatial.Face)
                {
                    faces.Add((Spatial.Face)geometry3D);
                    continue;
                }

                if (geometry3D is Spatial.IClosedPlanar3D)
                {
                    faces.Add(new Spatial.Face((Spatial.IClosedPlanar3D)geometry3D));
                    continue;
                }

                if(geometry3D is Spatial.ICurvable3D)
                {
                    List<Spatial.Point3D> point3Ds = ((Spatial.ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    faces.Add(new Spatial.Face(new Spatial.Polygon3D(point3Ds)));
                }
            }

            return faces;
        }

    }
}
