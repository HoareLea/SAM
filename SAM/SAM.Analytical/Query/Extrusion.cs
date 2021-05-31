using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Extrusion Extrusion(this Panel panel)
        {
            if (panel == null)
            {
                return null;
            }

            Construction construction = panel.Construction;
            if (construction == null)
            {
                return null;
            }

            double thickness = construction.GetThickness();
            if (double.IsNaN(thickness) || thickness == 0)
            {
                return null;
            }

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return null;
            }


            Geometry.Planar.IClosed2D externalEdge2D = face3D.ExternalEdge2D;
            if (externalEdge2D == null)
            {
                return null;
            }

            Vector3D vector3D_Extrusion = plane.Normal * thickness;
            
            Face3D face3D_Extrusion = null;
            switch (panel.PanelGroup)
            {
                case Analytical.PanelGroup.Floor:
                    face3D_Extrusion = face3D;
                    break;
                case Analytical.PanelGroup.Roof:
                    face3D_Extrusion = face3D;
                    break;
                default:
                    face3D_Extrusion = face3D.GetMoved(plane.Normal.GetNegated() * (thickness / 2)) as Face3D;
                    break;
            }

            if (face3D_Extrusion == null || vector3D_Extrusion == null)
                return null;

            return new Extrusion(face3D_Extrusion, vector3D_Extrusion);
        }
    }
}