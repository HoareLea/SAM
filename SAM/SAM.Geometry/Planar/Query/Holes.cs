using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<IClosed2D> Holes(this IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            List<Face2D> face2Ds_Union = Union(face2Ds);
            if (face2Ds_Union == null)
                return null;

            List<Tuple<BoundingBox2D, Face2D>> tuples = new List<Tuple<BoundingBox2D, Face2D>>();
            foreach(Face2D face2D in face2Ds_Union)
            {
                BoundingBox2D boundingBox2D = face2D?.GetBoundingBox();
                if (boundingBox2D == null)
                    continue;

                tuples.Add(new Tuple<BoundingBox2D, Face2D>(boundingBox2D, face2D));
            }

            List<IClosed2D> result = new List<IClosed2D>();
            foreach (Face2D face2D in face2Ds_Union)
            {
                List<IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
                if (closed2Ds == null || closed2Ds.Count == 0)
                    continue;

                foreach(IClosed2D closed2D in closed2Ds)
                {
                    BoundingBox2D boundingBox2D = closed2D?.GetBoundingBox(tolerance);
                    if (boundingBox2D == null)
                        continue;

                    Tuple<BoundingBox2D, Face2D> tuple = tuples.Find(x => boundingBox2D.InRange(x.Item1, tolerance));
                    if (tuple != null)
                        continue;

                    result.Add(closed2D);
                }
            }

            return result;
        }
    }
}