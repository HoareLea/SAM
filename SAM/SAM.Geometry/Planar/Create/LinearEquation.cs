using SAM.Math;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static LinearEquation LinearEquation(this Point2D point2D_1, Point2D point2D_2)
        {
            if(point2D_1 == point2D_2 || point2D_1 == null)
            {
                return null;
            }

            return Math.Create.LinearEquation(point2D_1.X, point2D_1.Y, point2D_2.X, point2D_2.Y);

        }
    }
}