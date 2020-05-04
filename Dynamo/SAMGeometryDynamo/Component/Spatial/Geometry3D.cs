using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAMGeometryDynamo
{
    /// <summary>
    /// SAM Geometry3D
    /// </summary>
    public static class Geometry3D
    {
        ///// <summary>
        ///// Snaps geometry to given SAM Point3Ds
        ///// </summary>
        ///// <param name="geometry3D">SAM Geometry to be snapped</param>
        ///// <param name="point3Ds">Snaping Points</param>
        ///// <param name="maxDistance">Max distance between geometry points and snaping point</param>
        ///// <returns name="geometry3D">Snapped 3D SAM Geometry</returns>
        ///// <search>
        ///// Snap, Geometry3D
        ///// </search>
        //public static IGeometry3D Snap(IGeometry3D geometry3D, IEnumerable<SAM.Geometry.Spatial.Point3D> point3Ds, double maxDistance)
        //{
        //    if (geometry3D is SAM.Geometry.Spatial.Point3D)
        //        return SAM.Geometry.Spatial.Point3D.Snap(point3Ds, (SAM.Geometry.Spatial.Point3D)geometry3D, maxDistance);

        // if (geometry3D is Segment3D) return Segment3D.Snap(point3Ds, (Segment3D)geometry3D, maxDistance);

        // if (geometry3D is Polygon3D) return SAM.Geometry.Spatial.Polygon3D.Snap(point3Ds,
        // (SAM.Geometry.Spatial.Polygon3D)geometry3D, maxDistance);

        //    return null;
        //}

        public static object Snap(object geometry, IEnumerable<object> points, double maxDistance)
        {
            ISAMGeometry3D geometry3D = null;

            if (geometry is ISAMGeometry3D)
                geometry3D = (ISAMGeometry3D)geometry;
            else if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                geometry3D = ((Autodesk.DesignScript.Geometry.Geometry)geometry).ToSAM() as ISAMGeometry3D;

            if (geometry3D == null)
                return null;

            List<SAM.Geometry.Spatial.Point3D> point3Ds = null;
            foreach (object object_point in points)
            {
                SAM.Geometry.Spatial.Point3D point3D = null;

                if (object_point is SAM.Geometry.Spatial.Point3D)
                    point3D = (SAM.Geometry.Spatial.Point3D)object_point;
                else if (object_point is Autodesk.DesignScript.Geometry.Point)
                    point3D = ((Autodesk.DesignScript.Geometry.Point)object_point).ToSAM();

                if (point3D == null)
                    continue;

                if (point3Ds == null)
                    point3Ds = new List<SAM.Geometry.Spatial.Point3D>();

                point3Ds.Add(point3D);
            }

            if (point3Ds == null)
                return null;

            ISAMGeometry3D geometry3D_Snapped = null;

            if (geometry3D is SAM.Geometry.Spatial.Point3D)
                geometry3D_Snapped = SAM.Geometry.Spatial.Point3D.Snap(point3Ds, (SAM.Geometry.Spatial.Point3D)geometry3D, maxDistance);

            if (geometry3D is Segment3D)
                geometry3D_Snapped = Segment3D.Snap(point3Ds, (Segment3D)geometry3D, maxDistance);

            if (geometry3D is Face3D)
                geometry3D = ((Face3D)geometry3D).GetExternalEdge() as SAM.Geometry.Spatial.Polygon3D;

            if (geometry3D is SAM.Geometry.Spatial.Polygon3D)
                geometry3D_Snapped = SAM.Geometry.Spatial.Polygon3D.Snap(point3Ds, (SAM.Geometry.Spatial.Polygon3D)geometry3D, maxDistance);

            if (geometry3D_Snapped == null)
                return null;

            if (geometry is ISAMGeometry3D)
                return geometry3D_Snapped;

            if (geometry is Autodesk.DesignScript.Geometry.Surface && geometry3D_Snapped is SAM.Geometry.Spatial.Polygon3D)
                geometry3D_Snapped = new Face3D((SAM.Geometry.Spatial.Polygon3D)geometry3D_Snapped);

            if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                return geometry3D_Snapped.ToDynamo();

            return null;
        }
    }
}