namespace SAMAnalyticalDynamo
{
    /// <summary>
    /// SAM Analytical Construction
    /// </summary>
    public static class Construction
    {
        /// <summary>
        /// Creates SAM Analytical Construction by SAM Point3Ds
        /// </summary>
        /// <param name="name">Construction Name</param>
        /// <returns name="construction">SAM Analytical Construction</returns>
        /// <search>SAM Analytical Construction, ByName</search>
        public static SAM.Analytical.Construction ByName(string name)
        {
            return new SAM.Analytical.Construction(name);
        }

        /// <summary>
        /// Gets Construction Name
        /// </summary>
        /// <param name="name">Construction Name</param>
        /// <returns name="construction">SAM Analytical Construction</returns>
        /// <search>SAM Analytical Construction, ByName</search>
        public static string Name(SAM.Analytical.Construction construction)
        {
            return construction.Name;
        }
    }
}