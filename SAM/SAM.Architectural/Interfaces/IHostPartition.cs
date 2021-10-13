using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public interface IHostPartition : IArchitecturalObject, ISAMObject, IFace3DObject, IPartition
    {
        List<IOpening> Openings { get; }

        bool AddOpening(IOpening opening, double tolerance = Tolerance.Distance);
    }
}
