using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public interface IHostPartition : IAnalyticalObject, ISAMObject, IFace3DObject, IPartition
    {
        List<IOpening> Openings { get; }

        bool AddOpening(IOpening opening, double tolerance = Tolerance.Distance);

        IOpening RemoveOpening(System.Guid guid);

        bool HasOpening(System.Guid guid);
    }
}
