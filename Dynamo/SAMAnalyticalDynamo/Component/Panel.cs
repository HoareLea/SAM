using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Analytical;
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
        /// <param name="point3Ds">SAM Point3Ds</param>
        /// <param name="construction">SAM Analytical Construction</param>
        /// <param name="panelType">Panel Type</param>
        /// <returns name="panel">SAM Analytical Panel</returns>
        /// <search>
        /// SAM Analytical Panel, ByPoint3Ds
        /// </search>
        public static SAM.Analytical.Panel ByPoint3Ds(IEnumerable<SAM.Geometry.Spatial.Point3D> point3Ds, SAM.Analytical.Construction construction, object panelType = null)
        {
            return new SAM.Analytical.Panel(construction, Query.PanelType(panelType), new SAM.Geometry.Spatial.Polygon3D(point3Ds));
        }

        /// <summary>
        /// Creates SAM Analytical Panel by given geometry
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="construction">SAM Analytical Construction</param>
        /// <param name="panelType">Panel Type</param>
        /// <search>
        /// ByGeometry, 
        /// </search>
        public static SAM.Analytical.Panel ByGeometry(object geometry, SAM.Analytical.Construction construction, object panelType  = null)
        {
            IGeometry3D geometry3D = geometry as IGeometry3D;
            if (geometry3D == null)
            {
                if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                    geometry3D = ((Autodesk.DesignScript.Geometry.Geometry)geometry).ToSAM() as IGeometry3D;
            }

            if (geometry3D == null)
                return null;

            IClosed3D closed3D = geometry3D as IClosed3D;
            if (closed3D == null)
                return null;

            return new SAM.Analytical.Panel(construction, Query.PanelType(panelType), closed3D);
        }

        public static SAM.Analytical.Panel SnapByPoints(SAM.Analytical.Panel panel, IEnumerable<object> points, double maxDistance = 0.2)
        {
            List<SAM.Geometry.Spatial.Point3D> point3Ds = new List<SAM.Geometry.Spatial.Point3D>();
            foreach(object @object in points)
            {
                if (@object is SAM.Geometry.Spatial.Point3D)
                {
                    point3Ds.Add((SAM.Geometry.Spatial.Point3D)@object);
                }
                else if(@object is Autodesk.DesignScript.Geometry.Point)
                {
                    point3Ds.Add(((Autodesk.DesignScript.Geometry.Point)@object).ToSAM());
                }
            }

            SAM.Analytical.Panel panel_New = new SAM.Analytical.Panel(panel);
            panel_New.Snap(point3Ds, maxDistance);

            return panel_New;
        }
    }
}
