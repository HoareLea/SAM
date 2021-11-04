namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Plane ToSAM(this global::Rhino.Geometry.Plane plane)
        {
            return new Spatial.Plane(plane.Origin.ToSAM(), plane.XAxis.ToSAM(), plane.YAxis.ToSAM());
        }
    }
}