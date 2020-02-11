using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface IClosed3D : IBoundable3D
    {
        IClosed3D GetExternalEdges();
    }
}
