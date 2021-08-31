using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool RemoveOverlapApertures(this Panel panel, out List<Aperture> removedApertures, double tolerance = Core.Tolerance.Distance)
        {
            removedApertures = null;
            
            List<Aperture> apertures = panel?.Apertures;
            if (apertures == null || apertures.Count < 2)
            {
                return false;
            }

            Plane plane = panel.Plane;
            if(plane == null)
            {
                return false;
            }

            List<Tuple<BoundingBox2D, Face2D, Aperture>> tuples = new List<Tuple<BoundingBox2D, Face2D, Aperture>>();
            foreach(Aperture aperture in apertures)
            {
                Face2D face2D = plane.Convert(plane.Project(aperture?.GetFace3D()));
                if(face2D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox2D, Face2D, Aperture>(face2D.GetBoundingBox(), face2D, aperture));
            }

            if(tuples == null || tuples.Count == 0)
            {
                return false;
            }

            for(int i=0; i < tuples.Count; i++)
            {
                Tuple<BoundingBox2D, Face2D, Aperture> tuple_1 = tuples[i];

                for (int j = 1; j < tuples.Count - 1; j++)
                {
                    Tuple<BoundingBox2D, Face2D, Aperture> tuple_2 = tuples[j];

                    if(!tuple_1.Item1.InRange(tuple_2.Item1, tolerance))
                    {
                        continue;
                    }

                    List<Face2D> face2Ds = tuple_1.Item2.Intersection(tuple_2.Item2, tolerance);
                    face2Ds?.RemoveAll(x => x == null || x.GetArea() < tolerance);
                    if(face2Ds == null || face2Ds.Count == 0)
                    {
                        continue;
                    }

                    removedApertures = new List<Aperture>();

                    if(tuple_1.Item2.GetArea() > tuple_2.Item2.GetArea())
                    {
                        removedApertures.Add(tuple_2.Item3);
                    }
                    else
                    {
                        removedApertures.Add(tuple_1.Item3);
                    }

                    foreach(Aperture aperture in removedApertures)
                    {
                        panel.RemoveAperture(aperture.Guid);
                    }
                    
                    RemoveOverlapApertures(panel, out List<Aperture> removedApertures_Temp, tolerance);
                    if(removedApertures_Temp != null && removedApertures_Temp.Count > 0)
                    {
                        removedApertures.AddRange(removedApertures_Temp);
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool RemoveOverlapApertures(this List<Panel> panels, out List<Aperture> removedApertures, double tolerance = Core.Tolerance.Distance)
        {
            removedApertures = null;

            if (panels == null || panels.Count == 0)
            {
                return false;
            }

            bool result = false;
            for(int i=0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if(panel == null)
                {
                    continue;
                }

                panel = new Panel(panel);
                if(!RemoveOverlapApertures(panel, out List<Aperture> removedApertures_Temp, tolerance))
                {
                    continue;
                }

                panels[i] = panel;
                result = true;
                
                if(removedApertures_Temp == null || removedApertures_Temp.Count == 0)
                {
                    continue;
                }

                if (removedApertures == null)
                {
                    removedApertures = new List<Aperture>();
                }

                removedApertures.AddRange(removedApertures_Temp);
            }

            return result;

        }

        public static bool RemoveOverlapApertures(this AdjacencyCluster adjacencyCluster, out List<Aperture> removedApertures, double tolerance = Core.Tolerance.Distance)
        {
            removedApertures = null;

            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return false;
            }

            if (!RemoveOverlapApertures(panels, out removedApertures, tolerance))
            {
                return false;
            }

            foreach(Panel panel in panels)
            {
                adjacencyCluster.AddObject(panel);
            }

            return true;
        }
    }
}