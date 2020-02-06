using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface IBoundable3D : ISAMGeometry3D
    {
        BoundingBox3D GetBoundingBox(double offset = 0);
    }
}
