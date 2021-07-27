using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void RemoveSimilar(this List<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(face2Ds == null || face2Ds.Count < 2)
            {
                return;
            }

            List<Tuple<BoundingBox2D, Face2D>> tuples = face2Ds.FindAll(x => x != null).ConvertAll(x => new Tuple<BoundingBox2D, Face2D>(x?.GetBoundingBox(), x));

            face2Ds.Clear();
            while (tuples.Count > 0)
            {
                Tuple<BoundingBox2D, Face2D> tuple = tuples[0];
                tuples.RemoveAt(0);

                face2Ds.Add(tuple.Item2);

                List<Tuple<BoundingBox2D, Face2D>> tuples_Similar = tuples.FindAll(x => tuple.Item1.InRange(x.Item1, tolerance));
                if(tuples_Similar.Count == 0)
                {
                    continue;
                }

                tuples_Similar.RemoveAll(x => !x.Item2.Similar(tuple.Item2, tolerance));
                if(tuples_Similar.Count == 0)
                {
                    continue;
                }

                tuples_Similar.ForEach(x => tuples.Remove(x));
            }
        }
    }
}