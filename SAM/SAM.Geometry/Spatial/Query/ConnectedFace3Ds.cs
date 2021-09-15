using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> ConnectedFace3Ds(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Distance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || face3Ds == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();

            if (face3Ds.Count() == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
            for(int i = face3Ds_Temp.Count - 1; i >= 0; i--)
            {
                Face3D face3D_Temp = face3Ds_Temp[i];
                
                if (face3D_Temp == null)
                {
                    face3Ds_Temp.RemoveAt(i);
                    continue;
                }

                if(face3D.Distance(face3D_Temp, tolerance_Angle, tolerance_Distance) > tolerance_Distance)
                {
                    continue;
                }

                result.Add(face3D_Temp);
                face3Ds_Temp.RemoveAt(i);
            }

            int count = result.Count;
            for(int i=0; i < count; i++)
            {
                List<Face3D> face3Ds_Connected = result[i].ConnectedFace3Ds(face3Ds_Temp, tolerance_Angle, tolerance_Distance);
                if (face3Ds_Connected == null || face3Ds_Connected.Count == 0)
                {
                    continue;
                }

                face3Ds_Temp.RemoveAll(x => face3Ds_Connected.Contains(x));
                result.AddRange(face3Ds_Connected);
            }

            return result;
        }
    }
}