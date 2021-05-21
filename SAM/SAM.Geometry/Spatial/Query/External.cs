using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Gets External BoundingBox3Ds for given boundingBox3Ds
        /// </summary>
        /// <param name="boundingBox3Ds">BoundingBox3Ds</param>
        /// <returns>External BoundingBox3Ds</returns>
        public static List<BoundingBox3D> External(IEnumerable<BoundingBox3D> boundingBox3Ds)
        {
            if (boundingBox3Ds == null)
                return null;

            int count = boundingBox3Ds.Count();
            if (count == 0)
                return new List<BoundingBox3D>();

            if (count == 1)
                return new List<BoundingBox3D>() { boundingBox3Ds.First() };

            HashSet<int> indexes = new HashSet<int>();

            for (int i = 0; i < count - 1; i++)
            {
                if (indexes.Contains(i))
                    continue;

                BoundingBox3D boundingBox3D_1 = boundingBox3Ds.ElementAt(i);

                for (int j = 1; j < count; j++)
                {
                    if (i == j || indexes.Contains(j))
                        continue;

                    BoundingBox3D boundingBox3D_2 = boundingBox3Ds.ElementAt(j);

                    if (boundingBox3D_1.Inside(boundingBox3D_2))
                        indexes.Add(j);
                }
            }

            List<BoundingBox3D> result = new List<BoundingBox3D>();
            for (int i = 0; i < count; i++)
            {
                if (indexes.Contains(i))
                    continue;

                result.Add(boundingBox3Ds.ElementAt(i));
            }

            return result;
        }
    }
}