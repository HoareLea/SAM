using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Inside(this BoundingBox3D boundingBox3D, ISAMGeometry3DObject sAMGeometry3DObject, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (boundingBox3D == null || sAMGeometry3DObject == null)
            {
                return false;
            }

            if (sAMGeometry3DObject is IEnumerable<ISAMGeometry3DObject>)
            {
                foreach (ISAMGeometry3DObject sAMGeometry3DObject_Temp in (IEnumerable<ISAMGeometry3DObject>)sAMGeometry3DObject)
                {
                    if (Inside(boundingBox3D, sAMGeometry3DObject_Temp, acceptOnEdge, tolerance))
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
                return boundingBox3D.Inside((Point3D)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            if (sAMGeometry3D is Segment3D)
            {
                return boundingBox3D.Inside((Segment3D)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            if (sAMGeometry3D is BoundingBox3D)
            {
                return boundingBox3D.Inside((BoundingBox3D)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            if (sAMGeometry3D is Shell)
            {
                return Geometry.Spatial.Query.Inside(boundingBox3D, (Shell)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            if (sAMGeometry3D is Face3D)
            {
                return Geometry.Spatial.Query.Inside(boundingBox3D, (Face3D)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            if (sAMGeometry3D is ISegmentable3D)
            {
                return Geometry.Spatial.Query.Inside(boundingBox3D, (ISegmentable3D)sAMGeometry3D, acceptOnEdge, tolerance);
            }

            return false;
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

            List<Face3D> face3Ds = Geometry.Spatial.Query.Inside(shell, dictionary.Keys, silverSpacing, tolerance);
            if (face3Ds == null)
            {
                return null;
            }

            return face3Ds.ConvertAll(x => dictionary[x]);

        }
    }
}