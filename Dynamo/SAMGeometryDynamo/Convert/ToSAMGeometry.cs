using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        public static object ToSAMGeometry(Autodesk.DesignScript.Geometry.Geometry geometry)
        {
            return geometry.ToSAM();
        }
    }
}
