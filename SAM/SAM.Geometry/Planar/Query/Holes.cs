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

            }

            List<IClosed2D> result = new List<IClosed2D>();

            return result;
        }
    }
}