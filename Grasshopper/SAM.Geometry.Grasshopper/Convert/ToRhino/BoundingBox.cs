namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.BoundingBox ToRhino(this Spatial.BoundingBox3D boundingBox3D)
        {
            return new Rhino.Geometry.BoundingBox(boundingBox3D.Min.ToRhino(), boundingBox3D.Max.ToRhino());
        }
    }
}