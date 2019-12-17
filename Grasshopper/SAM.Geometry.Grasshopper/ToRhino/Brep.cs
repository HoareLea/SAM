using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this Spatial.Face face)
        {
            Spatial.IClosed3D closed3D = face.ToClosed3D();
            if(closed3D is Spatial.Polygon3D)
            {
                Spatial.Polygon3D polygon3D = (Spatial.Polygon3D)closed3D;
                return Rhino.Geometry.Brep.CreateEdgeSurface(new List<Rhino.Geometry.Curve> { polygon3D.ToRhino_PolylineCurve() });
            }

            return null;
        }
    }
}
