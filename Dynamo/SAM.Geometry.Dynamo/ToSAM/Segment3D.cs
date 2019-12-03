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
        public static Spatial.Segment3D ToSAM(this Autodesk.DesignScript.Geometry.Line line)
        {
            return new Spatial.Segment3D(line.StartPoint.ToSAM(), line.EndPoint.ToSAM());
        }
    }
}
