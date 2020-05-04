using SAM.Geometry.Spatial;

namespace SAMAnalyticalDynamo
{
    /// <summary>
    /// SAM Analytical Panel
    /// </summary>
    public static class Space
    {
        /// <summary>
        /// Creates SAM Analytical Space by SAM Point3D
        /// </summary>
        /// <param name="name">Space name</param>
        /// <param name="location">SAM Loaction Point</param>
        /// <returns name="space">SAM Analytical Space</returns>
        /// <search>SAM Analytical Space, ByPoint</search>
        public static SAM.Analytical.Space ByPoint(string name, object location)
        {
            Point3D point3D = location as Point3D;
            if (point3D == null && location is Autodesk.DesignScript.Geometry.Point)
                point3D = SAMGeometryDynamo.Convert.ToSAM((Autodesk.DesignScript.Geometry.Point)location);

            if (point3D == null)
                return null;

            return new SAM.Analytical.Space(name, point3D);
        }
    }
}