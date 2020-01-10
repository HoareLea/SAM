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
            return new SAM.Analytical.Panel(construction, new SAM.Geometry.Spatial.Polygon3D(point3Ds));
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

            IClosed3D closed3D = geometry3D as IClosed3D;
            if (closed3D == null)
                return null;
           
            return new SAM.Analytical.Panel(null, closed3D);
        }
    }
}
