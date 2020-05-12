using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<List<Panel>> OverlapPanels(this IEnumerable<Panel> panels, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Tuple<Face3D, Panel>> tuples = panels.ToList().ConvertAll(x => new Tuple<Face3D, Panel>(x.GetFace3D(), x));

            List<List<Panel>> result = new List<List<Panel>>();
            foreach (Tuple<Face3D, Panel> tuple in tuples)
            {
                List<Tuple<Face3D, Panel>> tuples_Overlap = tuples.FindAll(x => x.Item1.Overlap(tuple.Item1, tolerance_Angle, tolerance_Distance));
                tuples_Overlap.Remove(tuple);

                result.Add(tuples_Overlap.ConvertAll(x => x.Item2));
            }
            return result;
        }
    }
}