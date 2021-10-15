namespace SAM.Architectural
{
    public interface ITerrain : IArchitecturalObject
    {
        bool Below(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool Below(IPartition partition, double tolerance = Core.Tolerance.Distance);

        bool On(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool On(IPartition partition, double tolerance = Core.Tolerance.Distance);
    }
}
