namespace SAM.Geometry.Planar
{
    public interface ISAMGeometry2D : ISAMGeometry
    {
        ISAMGeometry2D GetTransformed(ITransform2D transform2D);

        bool Transform(ITransform2D transform2D);
    }
}