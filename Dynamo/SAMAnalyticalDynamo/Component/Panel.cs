using SAM.Geometry.Spatial;
using SAMGeometryDynamo;
using System.Collections.Generic;
using System.Linq;

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
        /// <search>SAM Analytical Panel, ByPoint3Ds</search>
        /// <example>
        /// <code>
        /// //This is test for Example and should be replace with correct example
        /// int c = Math.Add(4, 5);
        /// if (c > 10)
        /// {
        ///     Console.WriteLine(c);
        /// }
        /// </code>
        /// </example>
        public static SAM.Analytical.Panel ByPoint3Ds(IEnumerable<SAM.Geometry.Spatial.Point3D> point3Ds, SAM.Analytical.Construction construction, object panelType = null)
        {
            Plane plane = Create.Plane(point3Ds, SAM.Core.Tolerance.Distance);

            return new SAM.Analytical.Panel(construction, SAM.Analytical.Query.PanelType(panelType), new Face3D(new SAM.Geometry.Spatial.Polygon3D(point3Ds.ToList().ConvertAll(x => plane.Project(x)))));
        }

        /// <summary>
        /// Creates SAM Analytical Panel by given geometry
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="construction">SAM Analytical Construction</param>
        /// <param name="panelType">Panel Type</param>
        /// <search>ByGeometry,</search>
        public static SAM.Analytical.Panel ByGeometry(object geometry, SAM.Analytical.Construction construction, object panelType = null)
        {
            ISAMGeometry3D geometry3D = geometry as ISAMGeometry3D;
            if (geometry3D == null)
            {
                if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                    geometry3D = ((Autodesk.DesignScript.Geometry.Geometry)geometry).ToSAM() as ISAMGeometry3D;
            }

            if (geometry3D == null)
                return null;

            IClosedPlanar3D closedPlanar3D = geometry3D as IClosedPlanar3D;
            if (closedPlanar3D == null)
                return null;

            return new SAM.Analytical.Panel(construction, SAM.Analytical.Query.PanelType(panelType), new Face3D(closedPlanar3D));
        }

        /// <summary>
        /// Creates SAM Analytical Panel by given geometry
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="constructionName">Construction Name</param>
        /// <search>ByGeometry,</search>
        public static SAM.Analytical.Panel ByGeometry(object geometry, string constructionName = "SIM_EXT_SLD Default")
        {
            ISAMGeometry3D geometry3D = geometry as ISAMGeometry3D;
            if (geometry3D == null)
            {
                if (geometry is Autodesk.DesignScript.Geometry.Geometry)
                    geometry3D = ((Autodesk.DesignScript.Geometry.Geometry)geometry).ToSAM() as ISAMGeometry3D;
            }

            if (geometry3D == null)
                return null;

            IClosedPlanar3D closedPlanar3D = geometry3D as IClosedPlanar3D;
            if (closedPlanar3D == null)
                return null;

            return new SAM.Analytical.Panel(new SAM.Analytical.Construction(constructionName), SAM.Analytical.PanelType.Undefined, new Face3D(closedPlanar3D));
        }

        public static SAM.Analytical.Panel SnapByPoints(SAM.Analytical.Panel panel, IEnumerable<object> points, double maxDistance = 0.2)
        {
            List<SAM.Geometry.Spatial.Point3D> point3Ds = new List<SAM.Geometry.Spatial.Point3D>();
            foreach (object @object in points)
            {
                if (@object is SAM.Geometry.Spatial.Point3D)
                {
                    point3Ds.Add((SAM.Geometry.Spatial.Point3D)@object);
                }
                else if (@object is Autodesk.DesignScript.Geometry.Point)
                {
                    point3Ds.Add(((Autodesk.DesignScript.Geometry.Point)@object).ToSAM());
                }
            }

            SAM.Analytical.Panel panel_New = new SAM.Analytical.Panel(panel);
            panel_New.Snap(point3Ds, maxDistance);

            return panel_New;
        }

        public static IEnumerable<SAM.Analytical.Panel> SnapByOffset(IEnumerable<SAM.Analytical.Panel> panels, double offset = 0.2, double maxDistance = 0)
        {
            return SAM.Analytical.Query.SnapByOffset(panels, offset, maxDistance);
        }

        public static SAM.Analytical.Panel SetConstruction(SAM.Analytical.Panel panel, SAM.Analytical.Construction construction)
        {
            if (construction == null || panel == null)
                return null;

            return new SAM.Analytical.Panel(panel, construction);
        }

        public static SAM.Analytical.Panel SetPanelType(SAM.Analytical.Panel panel, object panelType)
        {
            if (panel == null)
                return null;

            return new SAM.Analytical.Panel(panel, SAM.Analytical.Query.PanelType(panelType));
        }

        public static object PanelType(SAM.Analytical.Panel panel)
        {
            return panel.PanelType;
        }
    }
}