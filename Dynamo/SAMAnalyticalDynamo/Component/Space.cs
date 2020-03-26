

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
        /// <search>
        /// SAM Analytical Space, ByPoint
        /// </search>
        public static SAM.Analytical.Space ByPoint(string name, SAM.Geometry.Spatial.Point3D location)
        {
            return new SAM.Analytical.Space(name, location);
        }
    }
}
