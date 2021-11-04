using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static List<ISAMGeometry3D> ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
                return new List<ISAMGeometry3D>() { ((GH_Curve)geometricGoo).ToSAM(simplify) };

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            //if (geometricGoo is GH_Point)
            //    return new List<Spatial.ISAMGeometry3D>() { ((GH_Point)geometricGoo).ToSAM() };

            if (geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Mesh)
                return new List<ISAMGeometry3D>() { ((GH_Mesh)geometricGoo).ToSAM() };

            object @object = Convert.ToSAM(geometricGoo as dynamic);
            if (@object == null)
                return null;

            if (@object is ISAMGeometry3D)
                return new List<ISAMGeometry3D>() { (ISAMGeometry3D)@object };

            if (@object is IEnumerable)
            {
                List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
                foreach (object object_Temp in (IEnumerable)@object)
                    if (object_Temp is ISAMGeometry3D)
                        result.Add((ISAMGeometry3D)object_Temp);

                return result;
            }

            return null;
        }

        public static List<ISAMGeometry3D> ToSAM(this GH_Surface surface, bool simplify = true)
        {
            return Rhino.Convert.ToSAM(surface.Value, simplify);
        }
    }
}