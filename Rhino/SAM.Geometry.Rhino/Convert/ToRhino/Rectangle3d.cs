namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Rectangle3d ToRhino(this Spatial.Rectangle3D rectangle3D)
        {
            if (rectangle3D == null)
                return global::Rhino.Geometry.Rectangle3d.Unset;

            Spatial.Plane plane = new Spatial.Plane(rectangle3D.Origin, rectangle3D.WidthDirection, rectangle3D.HeightDirection);

            return new global::Rhino.Geometry.Rectangle3d(plane.ToRhino(), rectangle3D.Width, rectangle3D.Height);
        }

        public static global::Rhino.Geometry.Rectangle3d ToRhino(this Planar.Rectangle2D rectangle2D)
        {
            if(rectangle2D == null)
            {
                return global::Rhino.Geometry.Rectangle3d.Unset;
            }

            Spatial.Plane plane = Spatial.Plane.WorldXY;

            return ToRhino(Spatial.Query.Convert(plane, rectangle2D));
        }
    }
}