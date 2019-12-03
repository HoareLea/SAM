using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Dynamo
{
    public static partial class Convert
    {
        public static Autodesk.DesignScript.Geometry.Geometry ToDynamoGeometry(object geometry)
        {
            return ((IGeometry)geometry).ToDynamo();
        }
    }
}
