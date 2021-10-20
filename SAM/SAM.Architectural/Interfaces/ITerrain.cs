namespace SAM.Architectural
{
    public interface ITerrain : IArchitecturalObject
    {
        bool Below(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool Below(Geometry.Spatial.IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance);

        bool On(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool On(Geometry.Spatial.IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance);
    }
}
