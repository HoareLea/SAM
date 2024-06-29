using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public interface ISpace : ISAMGeometry3DObject, IAnalyticalObject
    {
        Point3D Location { get; }

        string Name { get; }
    }
}
