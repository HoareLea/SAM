namespace SAM.Geometry.Spatial
{
    public interface IShellObject : ISAMGeometry3DObject
    {
        Shell Shell { get; }
    }
}