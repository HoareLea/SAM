using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.PolyCurve ToDynamo(this SAM.Geometry.Spatial.ICurvable3D curvable3D)
        {
            List<Autodesk.DesignScript.Geometry.Curve> curves = new List<Autodesk.DesignScript.Geometry.Curve>();
            foreach (SAM.Geometry.Spatial.ICurve3D curve3D in curvable3D.GetCurves())
                curves.Add(curve3D.ToDynamo());

            return Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(curves);
        }
    }
}