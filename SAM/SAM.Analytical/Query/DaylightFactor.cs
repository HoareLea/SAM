using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double DaylightFactor(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if(adjacencyCluster == null || space == null)
            {
                return double.NaN;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if(panels == null || panels.Count == 0)
            {
                return double.NaN;
            }

            double area = 0;
            double result = 0;

            foreach (Panel panel in panels)
            {
                Face3D face3D_Panel = panel?.GetFace3D(true);
                if(face3D_Panel == null)
                {
                    continue;
                }


                double area_Panel = face3D_Panel.GetArea();
                if(double.IsNaN(area_Panel))
                {
                    continue;
                }

                area += area_Panel;

                BoundaryType boundaryType = adjacencyCluster.BoundaryType(panel);
                if(boundaryType != Analytical.BoundaryType.Exposed)
                {
                    continue;
                }

                double tilt = panel.GetTilt();
                if(double.IsNaN(tilt))
                {
                    continue;
                }

                List<Tuple<double, double>> tuples = new List<Tuple<double, double>>();
                if(panel.TryGetValue(PanelParameter.LightTransmittance, out double lightTransmittance) && !double.IsNaN(lightTransmittance) && lightTransmittance > 0)
                {
                    tuples.Add(new Tuple<double, double>(area_Panel, lightTransmittance));
                }

                List<Aperture> apertures = panel.Apertures;
                if(apertures != null && apertures.Count != 0)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        Face3D face3D_Aperture = aperture?.GetFace3D();
                        if(face3D_Aperture == null)
                        {
                            continue;
                        }

                        double area_Aperture = face3D_Aperture.GetArea();
                        if(double.IsNaN(area_Aperture))
                        {
                            continue;
                        }

                        if (aperture.TryGetValue(ApertureParameter.LightTransmittance, out lightTransmittance) && !double.IsNaN(lightTransmittance) && lightTransmittance > 0)
                        {
                            tuples.Add(new Tuple<double, double>(area_Aperture, lightTransmittance));
                        }

                    }
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                foreach(Tuple<double, double> tuple in tuples)
                {
                    if(tilt < 90)
                    {
                        double temp_1 = tilt / 90.0;
                        double temp_2 = temp_1 * 45.0;
                        double temp_3 = 90.0 - temp_2;
                        double temp_4 = temp_3 * tuple.Item2;
                        double temp_5 = temp_4 / 0.76;
                        double temp_6 = temp_5 * tuple.Item1;
                        result += temp_6;
                    }
                    else
                    {
                        double temp_1 = 45.0 * tuple.Item2;
                        double temp_2 = temp_1 / 0.76;
                        double temp_3 = temp_2 * tuple.Item1;
                        result += temp_3;
                    }
                }
            }

            if(area == 0 || result == 0)
            {
                return 0;
            }

            result = result / area;

            return result;
        }
    }
}