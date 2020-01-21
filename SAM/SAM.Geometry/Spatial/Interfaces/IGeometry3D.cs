using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface IGeometry3D : IGeometry
    {
        IGeometry3D GetMoved(Vector3D vector3D);
    }
}
