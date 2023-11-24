namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            return Geometry.Rhino.Convert.ToRhino_Brep(planarBoundary3D?.GetFace3D());
        }
    }
}