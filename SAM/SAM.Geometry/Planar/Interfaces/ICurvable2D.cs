using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public interface ICurvable2D : IBoundable2D
    {
        List<ICurve2D> GetCurves();
    }
}