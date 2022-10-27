namespace SAM.Geometry.Spatial
{
    public interface ISegmentable3DObject : ISAMGeometry3DObject
    {
        ISegmentable3D Segmentable3D { get; }
    }
}