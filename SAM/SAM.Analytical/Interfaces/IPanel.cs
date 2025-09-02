using SAM.Geometry.Object.Spatial;
using System;

namespace SAM.Analytical
{
    public interface IPanel : IFace3DObject, IAnalyticalObject
    {
        Guid Guid { get; }

        Construction Construction { get; }

        Guid TypeGuid { get; }
    }
}
