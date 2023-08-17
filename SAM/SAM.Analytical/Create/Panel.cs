using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Panel Panel(this Construction construction, PanelType panelType, Segment3D segment3D, double height)
        {
            if (construction == null)
                return null;

            Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(segment3D, height);
            if (polygon3D == null)
                return null;

            return new Panel(construction, panelType, new Face3D(polygon3D));
        }

        public static Panel Panel(this Panel panel, Segment3D segment3D, double height)
        {
            if (panel == null)
                return null;

            Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(segment3D, height);
            if (polygon3D == null)
                return null;

            return new Panel(panel.Guid, panel, new Face3D(polygon3D));
        }

        public static Panel Panel(this Construction construction, Segment3D segment3D, Vector3D vector3D, PanelType panelType = PanelType.Undefined)
        {
            if(vector3D == null || !vector3D.IsValid() || segment3D == null || !segment3D.IsValid())
            {
                return null;
            }
            
            Face3D face3D = new Face3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D }));

            if(panelType == PanelType.Undefined)
            {
                if(construction != null && construction.TryGetValue(ConstructionParameter.DefaultPanelType, out string text))
                {
                    panelType = Query.PanelType(text, false);
                }

                if(panelType == PanelType.Undefined)
                {
                    Vector3D normal = face3D?.GetPlane()?.Normal;
                    if(normal != null)
                    {
                        panelType = normal.PanelType();
                    }
                }
            }

            return Panel(construction, panelType, face3D);
        }

        public static Panel Panel(Construction construction, PanelType panelType, Face3D face3D)
        {
            if(face3D == null || panelType == PanelType.Undefined)
            {
                return null;
            }

            return new Panel(construction, panelType, face3D);
        }

        public static Panel Panel(Guid guid, Panel panel, Face3D face3D, IEnumerable<Aperture> apertures = null, bool trimGeometry = true, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
        {
            if(panel == null || face3D == null || guid == Guid.Empty)
            {
                return null;
            }

            return new Panel(guid, panel, face3D, apertures, trimGeometry, minArea, maxDistance);
        }
        
        public static Panel Panel(Panel panel)
        {
            if(panel == null)
            {
                return null;
            }

            return new Panel(panel);
        }

        public static Panel Panel(Panel panel, PanelType panelType)
        {
            if(panel == null)
            {
                return null;
            }

            return new Panel(panel, panelType);
        }

        public static Panel Panel(Panel panel, Construction construction)
        {
            if(panel == null)
            {
                return null;
            }
            
            return new Panel(panel, construction);
        }

        public static Panel Panel(Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
        {
            if(planarBoundary3D == null)
            {
                return null;
            }

            return new Panel(construction, panelType, planarBoundary3D);
        }

        /// <summary>
        /// Creates Panel without geometry
        /// </summary>
        /// <param name="construction">Construction</param>
        /// <param name="panelType">PanelType</param>
        /// <returns>Panel</returns>
        public static Panel Panel(Construction construction, PanelType panelType)
        {
            PlanarBoundary3D planarBoundary3D = null;

            return new Panel(construction, panelType, planarBoundary3D);
        }

        public static Panel Panel(Guid guid, Panel panel, PlanarBoundary3D planarBoundary3D)
        {
            if(guid == Guid.Empty || panel == null || planarBoundary3D == null)
            {
                return null;
            }
            return new Panel(guid, panel, planarBoundary3D);
        }
    }
}