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

        public static bool Inside(this BoundingBox3D boundingBox3D, ISegmentable3D segmentable3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if(boundingBox3D == null || segmentable3D == null)
            {
                return false;
            }

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return false;
            }

            return segment3Ds.Find(x => !boundingBox3D.Inside(x, acceptOnEdge, tolerance)) == null;
        }

        public static bool Inside(this BoundingBox3D boundingBox3D, Face3D face3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if(boundingBox3D == null || face3D == null)
            {
                return false;
            }

            IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
            if(closedPlanar3D == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if(segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }

            return Inside(boundingBox3D, segmentable3D, acceptOnEdge, tolerance);
        }

        public static bool Inside(this BoundingBox3D boundingBox3D, Shell shell, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if(boundingBox3D == null || shell == null)
            {
                return false;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null)
            {
                return false;
            }

            foreach(Face3D face3D in face3Ds)
            {
                if(face3D == null)
                {
                    continue;
                }

                if(!Inside(boundingBox3D, face3D, acceptOnEdge, tolerance))
                {
                    return false;
                }
            }

            return true;
        }
    }
}