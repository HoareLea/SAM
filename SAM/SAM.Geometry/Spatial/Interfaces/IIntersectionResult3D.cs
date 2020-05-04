using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public interface IIntersectionResult3D
    {
        bool Intersecting { get; }

        List<ISAMGeometry3D> Geometry3Ds { get; }
    }
}