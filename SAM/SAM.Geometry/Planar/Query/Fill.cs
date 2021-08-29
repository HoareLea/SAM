using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> Fill(this Face2D face2D, IEnumerable<Face2D> face2Ds, double offset = 0.1, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null || face2Ds == null)
            {
                return null;
            }

            List<Face2D> face2Ds_Temp = new List<Face2D>();
            foreach(Face2D face2D_Temp in face2Ds)
            {
                List<Face2D> face2Ds_Intersection = face2D.Intersection(face2D_Temp, tolerance);
                if(face2Ds_Intersection != null && face2Ds_Intersection.Count != 0)
                {
                    face2Ds_Temp.AddRange(face2Ds_Intersection);
                }
            }

            if(face2Ds_Temp == null || face2Ds_Temp.Count == 0)
            {
                return null;
            }

            if (face2Ds_Temp.Count == 1)
            {
                return new List<Face2D>() { new Face2D(face2D) };
            }

            Modify.MergeOverlaps(face2Ds_Temp, tolerance);

            double area = face2D.GetArea();
            double area_Temp = face2Ds_Temp.ConvertAll(x => x.GetArea()).Sum();

            if (System.Math.Abs(area - area_Temp) < tolerance)
            {
                return face2Ds_Temp;
            }

            List<Face2D> face2Ds_Offset = new List<Face2D>();
            foreach(Face2D face2D_Temp in face2Ds_Temp)
            {
                List<Face2D> face2Ds_Offset_Temp = face2D_Temp.Offset(offset, true, false, tolerance);
                if (face2Ds_Offset_Temp != null && face2Ds_Offset_Temp.Count != 0)
                {
                    face2Ds_Offset.AddRange(face2Ds_Offset_Temp);
                }
            }

            //Making sure Face2Ds do not reduce size
            face2Ds_Temp = new List<Face2D>(face2Ds_Offset);
            Modify.MergeOverlaps(face2Ds_Temp, tolerance);
            foreach(Face2D face2D_Temp in face2Ds_Temp)
            {
                Point2D point2D = face2D_Temp.InternalPoint2D(tolerance);
                int index = face2Ds_Offset.FindIndex(x => x.GetBoundingBox().Inside(point2D, tolerance) && x.Inside(point2D, tolerance));
                if(index == -1)
                {
                    continue;
                }

                List<Face2D> face2Ds_Union = face2D_Temp.Union(face2Ds_Offset[index], tolerance);
                if(face2Ds_Union == null || face2Ds_Union.Count == 0)
                {
                    continue;
                }

                if(face2Ds_Union.Count > 1)
                {
                    face2Ds_Union.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                }

                face2Ds_Offset[index] = face2Ds_Union[0];
            }

            return Fill(face2D, face2Ds_Offset, offset, tolerance);
        }
    }
}