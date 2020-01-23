using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    { 

        public static List<Spatial.IGeometry3D> ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
                return new List<Spatial.IGeometry3D>() { ((GH_Curve)geometricGoo).ToSAM(simplify)};

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Point)
                return new List<Spatial.IGeometry3D>() { ((GH_Point)geometricGoo).ToSAM() };

            if(geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            return (geometricGoo as dynamic).ToSAM();
        }

        public static List<Spatial.IGeometry3D> ToSAM(this Brep brep, bool simplify = true)
        {
            List<Spatial.IGeometry3D> result = new List<Spatial.IGeometry3D>();
            foreach (Surface surface in brep.Surfaces)
                result.Add(surface.ToSAM(simplify));
            return result;
        }

    }
}
