using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<ISegmentable3D> Inside(this Shell shell, IEnumerable<ISegmentable3D> segmentable3Ds, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || segmentable3Ds == null)
                return null;

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();

            List<ISegmentable3D> result = new List<ISegmentable3D>();

            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                if (point3Ds.Find(x => !boundingBox3D_Shell.Inside(x, true, tolerance)) != null)
                    continue;

                if (point3Ds.Find(x => !shell.Inside(x, silverSpacing, tolerance)) != null)
                    continue;

                result.Add(segmentable3D);
            }


            return result;
        }

        public static List<Face3D> Inside(this Shell shell, IEnumerable<Face3D> face3Ds, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || face3Ds == null)
                return null;

            Dictionary<ISegmentable3D, Face3D> dictionary = new Dictionary<ISegmentable3D, Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                dictionary[segmentable3D] = face3D;
            }

            List<ISegmentable3D> segmentable3Ds = Inside(shell, dictionary.Keys, silverSpacing, tolerance);
            if (segmentable3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                result.Add(dictionary[segmentable3D]);

            return result;
        }

        public static List<T> Inside<T>(this Shell shell, IEnumerable<T> face3DObjects, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance) where T : IFace3DObject
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

            List<Face3D> face3Ds = Inside(shell, dictionary.Keys, silverSpacing, tolerance);
            if (face3Ds == null)
            {
                return null;
            }

            return face3Ds.ConvertAll(x => dictionary[x]);

        }
    }
}