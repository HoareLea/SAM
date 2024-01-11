using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IExtrusionObject : ISAMGeometry3DObject
    {
        Extrusion Extrusion { get; }
    }
}