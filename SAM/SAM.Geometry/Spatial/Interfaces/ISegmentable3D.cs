using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface ISegmentable3D : IBoundable3D
    {
        List<Segment3D> GetSegments();
    }
}
