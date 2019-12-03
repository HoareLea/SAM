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
        public static Autodesk.DesignScript.Geometry.Line ToDynamo(this Spatial.Segment3D segment3D)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(segment3D[0].ToDynamo(), segment3D[1].ToDynamo());
        }
    }
}
