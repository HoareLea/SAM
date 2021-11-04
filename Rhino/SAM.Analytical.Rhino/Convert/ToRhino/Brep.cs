namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            return Geometry.Rhino.Convert.ToRhino_Brep(planarBoundary3D?.GetFace3D());
        }

        public static global::Rhino.Geometry.Brep ToRhino(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Geometry.Rhino.Convert.ToRhino_Brep(panel?.GetFace3D(cutApertures));
        }
    }
}