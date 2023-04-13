using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedArea(this Space space, AdjacencyCluster adjacencyCluster = null)
        {
            if(space == null)
            {
                return double.NaN;
            }

            if (space.TryGetValue(SpaceParameter.Area, out double result) && !double.IsNaN(result))
            {
                return result;
            }

            result = double.NaN;

            if(adjacencyCluster != null)
            {
                Shell shell = adjacencyCluster.Shell(space);
                if(shell != null)
                {
                    List<Face3D> face3Ds = shell.Section();
                    if(face3Ds != null && face3Ds.Count != 0)
                    {
                        result = 0;
                        foreach(Face3D face3D in face3Ds)
                        {
                            if(face3D == null)
                            {
                                continue;
                            }

                            double area = face3D.GetArea();
                            if(double.IsNaN(area))
                            {
                                continue;
                            }
                            result += area;
                        }

                        if(result == 0)
                        {
                            result = double.NaN;
                        }
                    }
                }

                if(double.IsNaN(result))
                {
                    List<Panel> panels = GeomericalFloorPanels(adjacencyCluster, space);
                    if (panels != null && panels.Count != 0)
                    {
                        List<Face3D> face3Ds = panels?.ConvertAll(x => x.GetFace3D());
                        if (face3Ds != null && face3Ds.Count != 0)
                        {
                            result = 0;
                            foreach (Face3D face3D in face3Ds)
                            {
                                if (face3D == null)
                                {
                                    continue;
                                }

                                double area = face3D.GetArea();
                                if (double.IsNaN(area))
                                {
                                    continue;
                                }
                                result += area;
                            }

                            if (result == 0)
                            {
                                result = double.NaN;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}