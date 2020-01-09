using System;
using System.Collections.Generic;

using SAM.Geometry.Spatial;

namespace SAMAnalyticalDynamo
{
    /// <summary>
    /// SAM Analytical Panel
    /// </summary>
    public static class Panel
    {
        /// <summary>
        /// Creates SAM Analytical Panel by SAM Point3Ds
        /// </summary>
        /// <param name="construction">SAM Analytical Construction</param>
        /// <param name="point3Ds">SAM Point3Ds</param>
        /// <returns name="panel">SAM Analytical Panel</returns>
        /// <search>
        /// SAM Analytical Panel, ByPoint3Ds
        /// </search>
        public static SAM.Analytical.Panel ByPoint3Ds(SAM.Analytical.Construction construction,  IEnumerable<Point3D> point3Ds)
        {
            List<Segment3D> segment3Ds = Point3D.GetSegments(point3Ds, true);

            return new SAM.Analytical.Panel(Guid.NewGuid(), construction, segment3Ds.ConvertAll(x => new SAM.Analytical.Edge(x)));
        }

        //public static SAM.Analytical.Panel BySAMGeometry(SAM.Analytical.Construction construction, IGeometry3D geometry3D)
        //{

        //}
    }
}
