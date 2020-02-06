using SAM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        public static Autodesk.DesignScript.Geometry.Geometry ToDynamoGeometry(object geometry)
        {
            return ((ISAMGeometry)geometry).ToDynamo();
        }
    }
}
