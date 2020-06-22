namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            return Geometry.Grasshopper.Convert.ToRhino_Brep(planarBoundary3D?.GetFace3D());
        }

        public static Rhino.Geometry.Brep ToRhino(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Geometry.Grasshopper.Convert.ToRhino_Brep(panel?.GetFace3D(cutApertures, tolerance));
        }
    }
}