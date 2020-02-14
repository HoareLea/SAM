namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            return Geometry.Grasshopper.Convert.ToRhino_Brep(planarBoundary3D.GetFace3D());
        }
    }
}
