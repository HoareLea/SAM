using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Fill(this Face3D face3D, IEnumerable<Face3D> face3Ds, double offset = 0.1, double tolerance = Core.Tolerance.Distance)
        {
            if(face3D == null || face3Ds == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face2D face2D = plane.Convert(face3D);

            List<Face2D> face2Ds = new List<Face2D>();
            
            foreach(Face3D face3D_Temp in face3Ds)
            {
                if(face3D_Temp == null)
                {
                    continue;
                }

                Face2D face2D_Temp = plane.Convert(plane.Project(face3D_Temp));
                if(face2D_Temp == null || !face2D_Temp.IsValid())
                {
                    continue;
                }

                if(face2D_Temp.GetArea() < tolerance)
                {
                    continue;
                }

                face2Ds.Add(face2D_Temp);

            }

            return Planar.Query.Fill(face2D, face2Ds, offset, tolerance)?.ConvertAll(x => plane.Convert(x));
        }
    }
}