using Autodesk.DesignScript.Runtime;
using System.Linq;

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