using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SAM.Geometry.Spatial.Polygon3D ToSAM(this Autodesk.DesignScript.Geometry.Polygon polygon)
        {
            return new SAM.Geometry.Spatial.Polygon3D(polygon.Points.ToList().ConvertAll(x => x.ToSAM()));
        }
    }
}
