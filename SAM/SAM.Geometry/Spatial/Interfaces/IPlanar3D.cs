using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface IPlanar3D: ISAMGeometry3D
    {
        Plane GetPlane();
    }
}
