using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

namespace SAM.Geometry.Dynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Polygon ToDynamo(this Spatial.Polygon3D polygon3D)
        {
            return Autodesk.DesignScript.Geometry.Polygon.ByPoints(polygon3D.Points.ConvertAll(x => x.ToDynamo()));
        }
    }
}
