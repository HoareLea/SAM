using NetTopologySuite.Geometries.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Point3D Point3D(this Math.Matrix matrix)
        {
            if (matrix == null)
                return null;

            if (matrix.RowCount() < 3 || matrix.ColumnCount() < 1)
                return null;

            return new Point3D(matrix[0, 0], matrix[1, 0], matrix[2, 0]);
        }
    }
}