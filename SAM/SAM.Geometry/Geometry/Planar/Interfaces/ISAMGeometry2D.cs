namespace SAM.Geometry.Planar
{
    public interface ISAMGeometry2D : ISAMGeometry
    {
        ISAMGeometry2D GetTransformed(Transform2D transform2D);

        bool Transform(Transform2D transform2D);
    }
}