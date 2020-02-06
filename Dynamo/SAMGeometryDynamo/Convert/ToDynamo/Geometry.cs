using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using SAM.Geometry;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Geometry ToDynamo(this ISAMGeometry geometry)
        {
            return Convert.ToDynamo(geometry as dynamic);
        }
    }
}
