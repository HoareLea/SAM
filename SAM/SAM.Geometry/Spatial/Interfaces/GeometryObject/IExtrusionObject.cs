namespace SAM.Geometry.Spatial
{
    public interface IExtrusionObject : ISAMGeometry3DObject
    {
        Extrusion Extrusion { get; }
    }
}