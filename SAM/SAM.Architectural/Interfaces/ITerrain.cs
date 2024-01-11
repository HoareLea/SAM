namespace SAM.Architectural
{
    public interface ITerrain : IArchitecturalObject
    {
        bool Below(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool Below(Geometry.Object.Spatial.IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance);

        bool On(Geometry.Spatial.Face3D face3D, double tolerance = Core.Tolerance.Distance);

        bool On(Geometry.Object.Spatial.IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance);
    }
}
