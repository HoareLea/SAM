using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D CalculatedInternalPoint3D(this Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || !shell.IsClosed(tolerance))
                return null;

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();
            if (boundingBox3D == null)
                return null;

            Dictionary<Face3D, Vector3D> dictionary = shell.NormalDictionary(true, silverSpacing, tolerance);
            if (dictionary == null || dictionary.Count == 0)
                return null;

            double distance = boundingBox3D.Min.Distance(boundingBox3D.Max);

            List<Tuple<Point3D, double>> tuples = new List<Tuple<Point3D, double>>();
            foreach(KeyValuePair<Face3D, Vector3D> keyValuePair in dictionary)
            {
                Vector3D vector3D = keyValuePair.Value.GetNegated() * distance;
                
                Point3D point3D = keyValuePair.Key.InternalPoint3D();

                Segment3D segment3D = new Segment3D(point3D, vector3D);

                List<Point3D> point3Ds = shell.IntersectionPoint3Ds(segment3D, false, tolerance);
                point3Ds?.RemoveAll(x => point3D.Distance(x) < tolerance);
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                point3Ds.SortByDistance(point3D);

                Point3D point3D_Mid = point3D.Mid(point3Ds[0]);
                if (point3D_Mid.Distance(point3D) < tolerance)
                    continue;

                if (tuples.Find(x => x.Item1.Distance(point3D_Mid) < silverSpacing) != null)
                    continue;

                double distance_Min = double.MaxValue;
                foreach(Face3D face3D in dictionary.Keys)
                {
                    double distance_Min_Temp = face3D.Distance(point3D_Mid);
                    if (distance_Min_Temp < distance_Min)
                        distance_Min = distance_Min_Temp;
                }

                tuples.Add(new Tuple<Point3D, double>(point3D_Mid, distance_Min));

            }

            Point3D point3D_Internal = shell.InternalPoint3D(silverSpacing, tolerance);
            if(point3D_Internal != null)
            {
                double distance_Min = double.MaxValue;
                foreach (Face3D face3D in dictionary.Keys)
                {
                    double distance_Min_Temp = face3D.Distance(point3D_Internal);
                    if (distance_Min_Temp < distance_Min)
                        distance_Min = distance_Min_Temp;
                }

                tuples.Add(new Tuple<Point3D, double>(point3D_Internal, distance_Min));
            }

            if (tuples.Count == 0)
                return null;
            
            tuples.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            return tuples[0].Item1;
        }
    }
}