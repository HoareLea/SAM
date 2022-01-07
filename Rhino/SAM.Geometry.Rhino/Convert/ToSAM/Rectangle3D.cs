namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Rectangle3D ToSAM(this global::Rhino.Geometry.Rectangle3d rectangle3d)
        {
            if(!rectangle3d.IsValid)
            {
                return null;
            }

            Spatial.Plane plane = rectangle3d.Plane.ToSAM();
            if(plane == null)
            {
                return null;
            }

            Planar.Rectangle2D rectangle2D = new Planar.Rectangle2D(new Planar.Point2D(rectangle3d.X.Min, rectangle3d.Y.Min), rectangle3d.Width, rectangle3d.Height);

            return new Spatial.Rectangle3D(plane, rectangle2D);
        }
    }
}