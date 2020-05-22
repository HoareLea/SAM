namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Thiness Ratio of closed Shape see: <see cref="https://tereshenkov.wordpress.com/2014/04/08/fighting-sliver-polygons-in-arcgis-thinness-ratio/)"/>. 
        /// Thinness is typically used to define the regularity of an object
        /// </summary>
        /// <param name="closed2D"> Closed 2D shape</param>
        /// <returns>ThinnessRatio</returns>
        public static double ThinnessRatio(this IClosed2D closed2D)
        {
            if (closed2D == null)
                return double.NaN;

            if(closed2D is ICurvable2D)
            {
                double area = closed2D.GetArea();
                if (double.IsNaN(area))
                    return double.NaN;

                double length = 0;
                ((ICurvable2D)closed2D).GetCurves().ForEach(x => length += x.GetLength());

                return (4 * System.Math.PI * area) / (length * length);
            }

            throw new System.NotImplementedException();
        }
    }
}