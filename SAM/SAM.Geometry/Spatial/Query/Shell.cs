using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Shell Shell(this Extrusion extrusion)
        {
            if (extrusion == null)
                return null;

            Vector3D vector3D = extrusion.Vector;
            if(vector3D == null)
            {
                return null;
            }

            Face3D face3D = extrusion.Face3D;
            if (face3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            face3Ds.Add(face3D);

            List<ISegmentable3D> segmentable3Ds = face3D.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                if(segment3Ds == null)
                {
                    continue;
                }

                foreach(Segment3D segment3D in segment3Ds)
                {
                    if(segment3D == null)
                    {
                        continue;
                    }

                    List<Point3D> point3Ds = new List<Point3D>();
                    point3Ds.Add(segment3D[0]);
                    point3Ds.Add(segment3D[1]);
                    point3Ds.Add(segment3D[1].GetMoved(vector3D) as Point3D);
                    point3Ds.Add(segment3D[0].GetMoved(vector3D) as Point3D);

                    face3Ds.Add(new Face3D(new Polygon3D(point3Ds)));
                }

            }
            face3Ds.Add(face3D.GetMoved(vector3D) as Face3D);

            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            return new Shell(face3Ds);
        }
    }
}