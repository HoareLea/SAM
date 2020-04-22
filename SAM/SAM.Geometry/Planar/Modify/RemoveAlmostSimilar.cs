using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;


namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void RemoveAlmostSimilar(this List<Polygon> polygons, double tolerance = Core.Tolerance.Distance)
        {
            if (polygons == null)
                return;

            HashSet<int> indexes_HashSet = new HashSet<int>(); 
            for(int i =0; i < polygons.Count - 1; i++)
            {
                if (indexes_HashSet.Contains(i))
                    continue;

                Polygon polygon_1 = polygons[i];
                
                for (int j = i + 1; j < polygons.Count - 1; j++)
                {
                    if (indexes_HashSet.Contains(j))
                        continue;

                    Polygon polygon_2 = polygons[j];

                    if (Query.AlmostSimilar(polygon_1, polygon_2, tolerance))
                        indexes_HashSet.Add(j);
                }
            }

            List<int> indexes_List = indexes_HashSet.ToList();
            indexes_List.Sort();
            indexes_List.Reverse();

            indexes_List.ForEach(x => polygons.RemoveAt(x));
        }
    }
}
