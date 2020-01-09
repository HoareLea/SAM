using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Creates SAM Analytical Panel by given geometry
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <returns name="panel">SAM Analytical Panel</returns>
        /// <search>
        /// ByGeometry, 
        /// </search>
        public static SAM.Analytical.Panel ByGeometry(object geometry)
        { 
            IEnumerable<SAM.Analytical.Edge> edges = null;
            if (geometry is IEnumerable<Point3D>)
            {
                edges = Point3D.GetSegments((IEnumerable<Point3D>)geometry, true).ConvertAll(x => new SAM.Analytical.Edge(x));
            }
            else if(geometry is IEnumerable<Segment3D>)
            {
                edges = ((IEnumerable<Segment3D>)geometry).ToList().ConvertAll(x => new SAM.Analytical.Edge(x));
            }
            else if(geometry is ISegmentable3D)
            {
                edges = ((ISegmentable3D)geometry).GetSegments().ConvertAll(x => new SAM.Analytical.Edge(x));
            }

            if (edges == null)
                return null;
           

            return new SAM.Analytical.Panel(Guid.NewGuid(), null, edges);
        }
    }
}
