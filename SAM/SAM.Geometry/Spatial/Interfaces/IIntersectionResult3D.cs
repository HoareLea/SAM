using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public interface IIntersectionResult3D
    {
        bool Intersecting { get; }

        List<ISAMGeometry3D> Geometry3Ds { get; }
    }
}
