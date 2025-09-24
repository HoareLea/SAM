using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> RemoveSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Point3D> point3Ds, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance)
        {
            if (adjacencyCluster == null || point3Ds == null || !point3Ds.Any())
            {
                return null;
            }

            List<Point3D> point3Ds_Temp = [.. point3Ds];
            point3Ds_Temp.RemoveAll(x => x is null);

            List<Space> result = [];

            if(point3Ds_Temp.Count == 0)
            {
                return result;
            }

            Dictionary<Space, Shell> dictionary = Query.ShellDictionary(adjacencyCluster);
            if (dictionary is null || dictionary.Count == 0)
            {
                return result;
            }

            List<Guid> guids = [];

            foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary)
            {
                if(keyValuePair.Value.GetBoundingBox() is not BoundingBox3D boundingBox3D )
                {
                    continue;
                }

                List<Point3D> point3Ds_Shell = point3Ds_Temp.FindAll(x => boundingBox3D.InRange(x, tolerance_Distance));
                if(point3Ds_Shell.Count == 0)
                {
                    continue;
                }

                Point3D point3D = point3Ds_Shell.Find(x => keyValuePair.Value.InRange(x, tolerance_Distance));
                if(point3D is null)
                {
                    continue;
                }

                List<Panel> panels = adjacencyCluster.GetPanels(keyValuePair.Key);
                if(panels != null && panels.Count > 0)
                {
                    foreach(Panel panel in panels)
                    {
                        List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                        if(spaces != null && spaces.Count == 1 && spaces[0].Guid == keyValuePair.Key.Guid)
                        {
                            adjacencyCluster.RemoveObject(panel);
                            continue;
                        }

                        guids.Add(panel.Guid);
                    }
                }

                adjacencyCluster.RemoveObject(keyValuePair.Key);
                result.Add(keyValuePair.Key);
            }

            if(guids == null || guids.Count == 0)
            {
                return result;
            }

            double groundElevation = 0;

            if (adjacencyCluster.GetGroundPanels() is List<Panel> groundPanels && groundPanels.Count > 0)
            {
                groundElevation = double.MinValue;
                foreach (Panel groundPanel in groundPanels)
                {
                    if (groundPanel?.GetBoundingBox() is not BoundingBox3D boundingBox3D_GroundPanel)
                    {
                        continue;
                    }

                    double groundElevation_Temp = boundingBox3D_GroundPanel.Max.Z;

                    if (groundElevation_Temp > groundElevation)
                    {
                        groundElevation = groundElevation_Temp;
                    }
                }

                if (groundElevation == double.MinValue)
                {
                    groundElevation = 0;
                }
            }

            if (adjacencyCluster.UpdatePanelTypes(groundElevation, guids) is IEnumerable<Panel> panels_Temp && panels_Temp.Any())
            {
                adjacencyCluster.UpdatePanelTypes(groundElevation, panels_Temp.ToList().ConvertAll(x => x.Guid));
            }

            adjacencyCluster.SetDefaultConstructionByPanelType(guids);

            return result;
        }

    }
}