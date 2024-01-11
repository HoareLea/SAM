using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void MergeOverlaps(this List<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(face2Ds == null || face2Ds.Count < 2)
            {
                return;
            }

            List<Tuple<Face2D, BoundingBox2D>> tuples = new List<Tuple<Face2D, BoundingBox2D>>();
            foreach(Face2D face2D in face2Ds)
            {
                BoundingBox2D boundingBox2D = face2D?.GetBoundingBox();
                if(boundingBox2D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<Face2D, BoundingBox2D>(face2D, boundingBox2D));
            }

            List<Face2D> face2Ds_Temp = new List<Face2D>();
            while (tuples.Count != 0)
            {
                Tuple<Face2D, BoundingBox2D> tuple = tuples[0];

                List<Tuple<Face2D, BoundingBox2D>> tuples_Temp = tuples.FindAll(x => tuple.Item2.InRange(x.Item2, tolerance));
                if(tuples_Temp.Count < 2)
                {
                    face2Ds_Temp.Add(tuple.Item1);
                    tuples.RemoveAt(0);
                    continue;
                }

                List<Face2D> face2Ds_Split = tuples_Temp.ConvertAll(x => x.Item1).Split(tolerance);
                List<Tuple<Face2D, List<Face2D>>> tuples_Split = new List<Tuple<Face2D, List<Face2D>>>();
                foreach(Face2D face2D_Split in face2Ds_Split)
                {
                    if(face2D_Split == null)
                    {
                        continue;
                    }

                    if(face2D_Split.GetArea() < tolerance)
                    {
                        continue;
                    }

                    Point2D point2D = face2D_Split.InternalPoint2D(tolerance);
                    List<Tuple<Face2D, BoundingBox2D>> tuples_Temp_Inside = tuples_Temp.FindAll(x => x.Item2.InRange(point2D, tolerance) && x.Item1.Inside(point2D, tolerance));
                    if (tuples_Temp_Inside == null || tuples_Temp_Inside.Count < 2)
                    {
                        continue;
                    }

                    tuples_Split.Add(new Tuple<Face2D, List<Face2D>>(face2D_Split, tuples_Temp_Inside.ConvertAll(x => x.Item1)));
                }

                if (tuples_Split.Count == 0)
                {
                    face2Ds_Temp.Add(tuple.Item1);
                    tuples.RemoveAt(0);
                    continue;
                }

                List<Tuple<double, List<Face2D>, List<Face2D>>> tuples_InvertedThinnessRatio = new List<Tuple<double, List<Face2D>, List<Face2D>>>();
                foreach(Tuple<Face2D, List<Face2D>> tuple_Split in tuples_Split)
                {
                    Face2D face2D_Split = tuple_Split.Item1;

                    foreach(Face2D face2D in tuple_Split.Item2)
                    {
                        List<Face2D> face2Ds_Split_Before = new List<Face2D>(tuple_Split.Item2);
                        face2Ds_Split_Before.Remove(face2D);

                        List<Face2D> face2D_Split_After = new List<Face2D>() { face2D };
                        foreach (Face2D face2D_Split_Before in face2Ds_Split_Before)
                        {
                            List<Face2D> face2D_Split_After_Temp = face2D_Split_Before.Difference(face2D_Split, tolerance);
                            if(face2D_Split_After_Temp != null && face2D_Split_After_Temp.Count != 0)
                            {
                                face2D_Split_After.AddRange(face2D_Split_After_Temp);
                            }
                        }

                        double thinnessRatio = face2D_Split_After.ConvertAll(x => 1 - x.ThinnessRatio()).Sum();

                        face2Ds_Split_Before.Add(face2D);

                        tuples_InvertedThinnessRatio.Add(new Tuple<double, List<Face2D>, List<Face2D>>(thinnessRatio, face2Ds_Split_Before, face2D_Split_After));
                    }


                }

                tuples_InvertedThinnessRatio.Sort((x, y) => x.Item1.CompareTo(y.Item1));

                Tuple<double, List<Face2D>, List<Face2D>> tuple_InvertedThinnessRatio = tuples_InvertedThinnessRatio[0];

                tuples.RemoveAt(0);

                face2Ds.Clear();
                face2Ds.AddRange(face2Ds_Temp);
                foreach(Tuple<Face2D, BoundingBox2D> tuple_Temp in tuples)
                {
                    if(tuple_InvertedThinnessRatio.Item2.Contains(tuple_Temp.Item1))
                    {
                        continue;
                    }

                    face2Ds.Add(tuple_Temp.Item1);
                }

                face2Ds.AddRange(tuple_InvertedThinnessRatio.Item3);

                MergeOverlaps(face2Ds, tolerance);
                return;
            }
        }
    }
}