using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.IClosed3D ToSAM(this BrepLoop brepLoop, bool simplify = true)
        {
            if (brepLoop.Face.IsPlanar(Core.Tolerance.Distance))
            {
                Spatial.ISAMGeometry3D sAMGeometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                if (sAMGeometry3D is Spatial.IClosedPlanar3D)
                    return new Spatial.Face3D(sAMGeometry3D as Spatial.IClosedPlanar3D);

                if (sAMGeometry3D is Spatial.ICurvable3D)
                {
                    List<Spatial.ICurve3D> curves = ((Spatial.ICurvable3D)sAMGeometry3D).GetCurves();
                    if (curves.TrueForAll(x => x is Spatial.Segment3D))
                        return new Spatial.Face3D(new Spatial.Polygon3D(curves.ConvertAll(x => x.GetStart())));
                }
            }
            else
            {
                Spatial.ISAMGeometry3D geometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                if (geometry3D is Spatial.Polyline3D)
                {
                    Spatial.PolycurveLoop3D polycurveLoop3D = new Spatial.PolycurveLoop3D(((Spatial.Polyline3D)geometry3D).GetSegments());
                    return new Spatial.Surface(polycurveLoop3D);
                }
            }

            return null;
        }
    }
}