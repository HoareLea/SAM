using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Dynamo
{
    /// <summary>
    /// SAM Polygon3D
    /// </summary>
    public static class Polygon3D
    {
        /// <summary>
        /// Creates SAM Polygon3D by SAM Point3D
        /// </summary>
        /// <param name="point3Ds">SAM Point3D</param>
        /// <returns name="polygon3D">SAM polygon3D</returns>
        /// <search>
        /// Polygon3D, ByPoints
        /// </search>
        public static Spatial.Polygon3D ByPoint3Ds(this IEnumerable<Spatial.Point3D> point3Ds)
        {
            return new Spatial.Polygon3D(point3Ds);
        }

        /// <summary>
        /// Creates SAM Polygon3D by Dynamo Points
        /// </summary>
        /// <param name="points">Dynamo Points</param>
        /// <returns name="polygon3D">SAM polygon3D</returns>
        /// <search>
        /// Polygon3D, ByPoints
        /// </search>
        public static Spatial.Polygon3D ByPoints(this IEnumerable<Autodesk.DesignScript.Geometry.Point> points)
        {
            List<Spatial.Point3D> pointList = new List<Spatial.Point3D>();
            foreach (Autodesk.DesignScript.Geometry.Point point in points)
                pointList.Add(Point3D.ByPoint(point));

            return new SAM.Geometry.Spatial.Polygon3D(pointList);
        }

                /// <summary>
        /// Converts SAM Polygon3D To Dynamo Polygon
        /// </summary>
        /// <param name="polygon3D">SAM polygon3D</param>
        /// <returns name="polygon">Dynamo Polygon</returns>
        /// <search>
        /// ToPolygon, Polygon
        /// </search>
        public static Autodesk.DesignScript.Geometry.Polygon ToPolygon(this Spatial.Polygon3D polygon3D)
        {
            List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (Spatial.Point3D point3D in polygon3D.Points)
                pointList.Add(Point3D.ToPoint(point3D));

            return Autodesk.DesignScript.Geometry.Polygon.ByPoints(pointList);
        }
    }

}
