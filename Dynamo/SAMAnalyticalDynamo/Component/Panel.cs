using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;
using SAMGeometryDynamo;

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
        public static SAM.Analytical.Panel ByPoint3Ds(SAM.Analytical.Construction construction,  IEnumerable<SAM.Geometry.Spatial.Point3D> point3Ds)
        {
            List<Segment3D> segment3Ds = SAM.Geometry.Spatial.Point3D.GetSegments(point3Ds, true);

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
            IGeometry3D geometry3D = geometry as IGeometry3D;
            if(geometry3D == null)
            {
                if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                    geometry3D = ((Autodesk.DesignScript.Geometry.Geometry)geometry).ToSAM() as IGeometry3D;
            }

            if (geometry3D == null)
                return null;


            IEnumerable<SAM.Analytical.Edge> edges = null;
            if (geometry3D is IEnumerable<SAM.Geometry.Spatial.Point3D>)
            {
                edges = SAM.Geometry.Spatial.Point3D.GetSegments((IEnumerable<SAM.Geometry.Spatial.Point3D>)geometry3D, true).ConvertAll(x => new SAM.Analytical.Edge(x));
            }
            else if(geometry3D is IEnumerable<Segment3D>)
            {
                edges = ((IEnumerable<Segment3D>)geometry3D).ToList().ConvertAll(x => new SAM.Analytical.Edge(x));
            }
            else if(geometry3D is ISegmentable3D)
            {
                edges = ((ISegmentable3D)geometry3D).GetSegments().ConvertAll(x => new SAM.Analytical.Edge(x));
            }
            else if(geometry3D is Face)
            {
                edges = (((Face)geometry3D).ToClosed3D() as SAM.Geometry.Spatial.Polygon3D).GetSegments().ConvertAll(x => new SAM.Analytical.Edge(x));
            }

            if (edges == null)
                return null;
           
            return new SAM.Analytical.Panel(Guid.NewGuid(), null, edges);
        }
    }
}
