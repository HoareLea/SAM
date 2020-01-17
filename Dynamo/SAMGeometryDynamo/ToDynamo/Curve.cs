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
        public static Autodesk.DesignScript.Geometry.Curve ToDynamo(this SAM.Geometry.Spatial.ICurve3D curve3D)
        {
            return ToDynamo(curve3D as dynamic);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Curve ToDynamo(this SAM.Geometry.Spatial.IClosed3D closed3D)
        {
            if (closed3D is SAM.Geometry.Spatial.ICurvable3D)
                return ((SAM.Geometry.Spatial.ICurvable3D)closed3D).ToDynamo();
            
            return null;
        }
    }
}
