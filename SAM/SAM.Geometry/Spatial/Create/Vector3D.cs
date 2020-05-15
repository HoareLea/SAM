using NetTopologySuite.Geometries.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Vector3D Vector3D(this Math.Matrix matrix)
        {
            if (matrix == null)
                return null;

            if (matrix.RowCount() < 3 || matrix.ColumnCount() < 1)
                return null;

            return new Vector3D(matrix[0, 0], matrix[1, 0], matrix[2, 0]);
        }
    }
}