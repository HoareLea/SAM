using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<double, List<Panel>> ElevationDictionary(this IEnumerable<Panel> panels, out double maxElevation, double tolerance = Core.Tolerance.Distance)
        {
            maxElevation = double.NaN;
            
            if (panels == null)
                return null;

            List<Tuple<double, List<Panel>>> tuples_Elevation = new List<Tuple<double, List<Panel>>>();
            foreach (Panel panel in panels)
            {
                BoundingBox3D boundingBox3D = panel?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double minElevation = boundingBox3D.Min.Z;
                Tuple<double, List<Panel>> tuple = tuples_Elevation.Find(x => System.Math.Abs(x.Item1 - minElevation) < tolerance);
                if (tuple == null)
                {
                    tuple = new Tuple<double, List<Panel>>(minElevation, new List<Panel>());
                    tuples_Elevation.Add(tuple);
                }

                double maxElevation_Temp = boundingBox3D.Max.Z;
                if(double.IsNaN(maxElevation) || maxElevation < maxElevation_Temp)
                {
                    maxElevation = maxElevation_Temp;
                }

                tuple.Item2.Add(panel);
            }

            tuples_Elevation.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            Dictionary<double, List<Panel>> result = new Dictionary<double, List<Panel>>();
            tuples_Elevation.ForEach(x => result[x.Item1] = x.Item2);

            return result;
        }

        public static Dictionary<double, List<Panel>> ElevationDictionary(this IEnumerable<Panel> panels, double tolerance = Core.Tolerance.Distance)
        {
            return ElevationDictionary(panels, out double maxElevation, tolerance);
        }
    }
}