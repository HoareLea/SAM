using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Rhino.Plugin
{
    public static partial class Modify
    {
        public static void SetBucketSizes(this List<Panel> panels, double minBucketSize = 0.2)
        {
            if (panels == null)
            {
                return;
            }

            List<Tuple<int, double>> tuples = new List<Tuple<int, double>>();
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if(panel == null)
                {
                    continue;
                }

                panels[i].SetValue(PanelParameter.BucketSize, minBucketSize);

                double thickness = double.NaN;

                Construction construction = panel.Construction;
                if (construction != null)
                {
                    thickness = construction.GetThickness();
                }
                else
                {
                    if (!construction.TryGetValue(ConstructionParameter.DefaultThickness, out thickness))
                    {
                        thickness = double.NaN;
                    }
                }

                if(double.IsNaN(thickness))
                {
                    thickness = 0;
                }

                tuples.Add(new Tuple<int, double>(i, thickness));
            }

            double max = tuples.ConvertAll(x => x.Item2).Max();
            double min = tuples.ConvertAll(x => x.Item2).Min();

            if(max > minBucketSize)
            {
                foreach (Tuple<int, double> tuple in tuples)
                {
                    double backetSize = Math.Query.Remap(tuple.Item2, min, max, minBucketSize, max);

                    panels[tuple.Item1].SetValue(PanelParameter.BucketSize, backetSize);
                }
            }
        }
    }
}