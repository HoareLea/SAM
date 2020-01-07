using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <search>
        /// SAM Analytical Construction, ByName
        /// </search>
        public static SAM.Analytical.Construction ByName(string name)
        {
            return new SAM.Analytical.Construction(name);
        }
    }
}
