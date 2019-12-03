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
        public static Spatial.Point3D ToSAM(this Autodesk.DesignScript.Geometry.Point point)
        {
            return new Spatial.Point3D(point.X, point.Y, point.Z);
        }
    }
}
