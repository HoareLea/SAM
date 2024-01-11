using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<IClosed2D> Edges(this IEnumerable<Face2D> face2Ds)
        {
            if (face2Ds == null)
                return null;

            List<IClosed2D> result = new List<IClosed2D>();
            foreach(Face2D face2D in face2Ds)
            {
                if(face2D == null)
                {
                    continue;
                }

                List<IClosed2D> edges_Temp = face2D.Edge2Ds;
                if(edges_Temp == null && edges_Temp.Count == 0)
                {
                    continue;
                }

                edges_Temp.ForEach(x => result.Add(x));
            }

            return result;
        }
   }
}