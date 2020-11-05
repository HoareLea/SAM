using SAM.Geometry.Spatial;

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
    }
}