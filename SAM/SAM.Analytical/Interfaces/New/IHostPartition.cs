using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public interface IHostPartition : IAnalyticalObject, IParameterizedSAMObject, IFace3DObject, IPartition, ISAMObject
    {
        List<T> GetOpenings<T>() where T : IOpening;

        List<IOpening> GetOpenings();

        List<IOpening> AddOpening(IOpening opening, double tolerance = Tolerance.Distance);

        IOpening RemoveOpening(System.Guid guid);

        IOpening GetOpening(System.Guid guid);

        bool HasOpening(System.Guid guid);

        List<Face3D> GetFace3Ds(bool cutOpenings = false, double tolerance = Tolerance.Distance);
    }
}
