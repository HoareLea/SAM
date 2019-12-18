using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface ICurve3D : IBoundable3D
    {
        Point3D GetStart();
    
        Point3D GetEnd();

        void Reverse();
    }
}
