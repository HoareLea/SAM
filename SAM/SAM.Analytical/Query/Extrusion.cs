using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Extrusion Extrusion(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null)
            {
                return null;
            }

            Construction construction = panel.Construction;
            if(construction == null)
            {
                return null;
            }

            double thickness = construction.GetThickness();
            if(double.IsNaN(thickness) || thickness == 0)
            {
                return null;
            }

            Face3D face3D = panel.GetFace3D();
            if(face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }


            Geometry.Planar.IClosed2D externalEdge2D = face3D.ExternalEdge2D;
            if(externalEdge2D == null)
            {
                return null;
            }

            bool rectangular = Geometry.Planar.Query.Rectangular(externalEdge2D, out Geometry.Planar.Rectangle2D rectangle2D, tolerance);
            if(rectangular)
            {
                List<Geometry.Planar.IClosed2D> internalEdge2Ds = face3D.InternalEdge2Ds;
                if (internalEdge2Ds != null && internalEdge2Ds.Count != 0)
                    rectangular = false;
            }

            if(rectangular)
            {
                Vector3D xAxis = plane.Convert(rectangle2D.WidthDirection);
                Vector3D yAxis = plane.Normal;

                throw new System.NotImplementedException();
            }
            else
            {
                Vector3D vector3D = plane.Normal * thickness;
                Face3D face3D_Profile = face3D.GetMoved(vector3D.GetNegated() * (thickness / 2)) as Face3D;

                return new Extrusion(face3D_Profile, vector3D);
            }

            return null;
        }
    }
}