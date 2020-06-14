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

            IClosed2D closed2D_Temp = closed2D;
            if(closed2D_Temp is Face)
                closed2D_Temp = ((Face)closed2D_Temp).ExternalEdge;

            if(closed2D_Temp is ICurvable2D)
            {
                double area = closed2D_Temp.GetArea();
                if (double.IsNaN(area))
                    return double.NaN;

                double length = 0;
                ((ICurvable2D)closed2D_Temp).GetCurves().ForEach(x => length += x.GetLength());

                return (4 * System.Math.PI * area) / (length * length);
            }

            throw new System.NotImplementedException();
        }
    }
}