using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public interface ISegmentable3D : ICurvable3D
    {
        List<Segment3D> GetSegments();
    }
}
