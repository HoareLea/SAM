using SAM.Core;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool InRange(this BoundingBox3D boundingBox3D, ISAMGeometry3DObject sAMGeometry3DObject, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D == null || sAMGeometry3DObject == null)
            {
                return false;
            }

            if (sAMGeometry3DObject is IEnumerable<ISAMGeometry3DObject>)
            {
                foreach (ISAMGeometry3DObject sAMGeometry3DObject_Temp in (IEnumerable<ISAMGeometry3DObject>)sAMGeometry3DObject)
                {
                    if (InRange(boundingBox3D, sAMGeometry3DObject_Temp, tolerance))
                    {
                        return true;
                    }
                }

                return false;
            }

            ISAMGeometry3D sAMGeometry3D = sAMGeometry3DObject.ISAMGeometry3D();
            if (sAMGeometry3D == null)
            {
                return false;
            }

            if (sAMGeometry3D is Point3D)
            {
                return boundingBox3D.InRange((Point3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is Segment3D)
            {
                return boundingBox3D.InRange((Segment3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is BoundingBox3D)
            {
                return boundingBox3D.InRange((BoundingBox3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is Shell)
            {
                return Geometry.Spatial.Query.InRange((Shell)sAMGeometry3D, boundingBox3D, tolerance);
            }

            if (sAMGeometry3D is Face3D)
            {
                return Geometry.Spatial.Query.InRange(boundingBox3D, (Face3D)sAMGeometry3D, tolerance);
            }

            if (sAMGeometry3D is ISegmentable3D)
            {
                return Geometry.Spatial.Query.InRange(boundingBox3D, (ISegmentable3D)sAMGeometry3D, tolerance);
            }

            return false;
        }

        public static List<T> InRange<T>(this Shell shell, IEnumerable<T> face3DObjects, double tolerance = Tolerance.Distance) where T : IFace3DObject
        {
            if (shell == null || face3DObjects == null)
            {
                return null;
            }

            Dictionary<Face3D, T> dictionary = new Dictionary<Face3D, T>();
            foreach (T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if (face3D == null)
                {
                    continue;
                }
                dictionary[face3D] = face3DObject;
            }

            List<Face3D> face3Ds = Geometry.Spatial.Query.InRange(shell, dictionary.Keys, tolerance);
            if (face3Ds == null)
            {
                return null;
            }

            return face3Ds.ConvertAll(x => dictionary[x]);

        }

        public static List<T> InRange<T>(this IEnumerable<T> face3DObjects, Point3D point3D, Range<double> range, bool sort = true, int count = int.MaxValue, double tolerance = Tolerance.Distance) where T : IFace3DObject
        {
            if (face3DObjects == null || point3D == null || range == null)
            {
                return null;
            }

            List<Tuple<double, T>> tuple = new List<Tuple<double, T>>();
            foreach (T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;

                if (face3D == null)
                {
                    continue;
                }

                double distance = face3D.Distance(point3D, tolerance);

                if (!range.In(distance))
                {
                    continue;
                }

                tuple.Add(new Tuple<double, T>(distance, face3DObject));

                if (tuple.Count >= count)
                {
                    return tuple.ConvertAll(x => x.Item2);
                }
            }

            return tuple.ConvertAll(x => x.Item2);
        }
    }
}