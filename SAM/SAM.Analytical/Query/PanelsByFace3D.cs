using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> PanelsByFace3D(this IEnumerable<Panel> panels, Face3D face3D, double areaFactor, double maxDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return PanelsByFace3D(panels, face3D, areaFactor, maxDistance, out List<double> intersectionAreas, tolerance_Angle, tolerance_Distance);
        }

        public static List<Panel> PanelsByFace3D(this IEnumerable<Panel> panels, Face3D face3D, double areaFactor, double maxDistance, out List<double> intersectionAreas, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            intersectionAreas = null;
            
            if (panels == null || face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Geometry.Planar.Face2D face2D = plane.Convert(face3D);

            double area = face2D.GetArea();
            if(area < tolerance_Distance)
            {
                return null;
            }

            List<Tuple<Panel, double>> tuples = new List<Tuple<Panel, double>>(); 
            foreach(Panel panel in panels)
            {
                Face3D face3D_Panel = panel?.GetFace3D();
                if(face3D_Panel == null)
                {
                    continue;
                }

                Plane plane_Panel = face3D_Panel.GetPlane();
                if(plane_Panel == null)
                {
                    continue;
                }

                if(plane.Normal.SmallestAngle(plane_Panel.Normal) > tolerance_Angle)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D_Panel = face3D_Panel.GetBoundingBox();
                if(boundingBox3D_Panel == null)
                {
                    continue;
                }

                if(!boundingBox3D.InRange(boundingBox3D_Panel, maxDistance))
                {
                    continue;
                }

                double distance = face3D.Distance(face3D_Panel, tolerance_Angle, tolerance_Distance);
                if(distance > maxDistance)
                {
                    continue;
                }

                Geometry.Planar.Face2D face2D_Panel = plane.Convert(plane.Project(face3D_Panel));
                if(face3D_Panel == null)
                {
                    continue;
                }

                List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Query.Intersection(face2D, face2D_Panel, tolerance_Distance);
                if(face2Ds == null || face2Ds.Count == 0)
                {
                    continue;
                }

                double area_Intersection = face2Ds.ConvertAll(x => x.GetArea()).Sum();
                if(area_Intersection / area < areaFactor)
                {
                    continue;
                }

                tuples.Add(new Tuple<Panel, double>(panel, area_Intersection));

            }

            if(tuples == null || tuples.Count == 0)
            {
                return null;
            }

            if (tuples.Count > 1)
            {
                tuples.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            }

            intersectionAreas = tuples.ConvertAll(x => x.Item2);
            return tuples.ConvertAll(x => x.Item1);
        }
    }
}