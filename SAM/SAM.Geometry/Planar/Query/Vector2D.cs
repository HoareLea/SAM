namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Calculate unit vector of angle provided
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public static Vector2D Vector2D(double angle)
        {
            if(double.IsNaN(angle))
            {
                return null;
            }

            return new Vector2D(System.Math.Cos(angle), System.Math.Sin(angle));
        }
    }
}