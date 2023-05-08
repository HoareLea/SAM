namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Thinness Ratio of closed Shape. Thinness is typically used to define the regularity of an object.
        /// For more information, see: https://tereshenkov.wordpress.com/2014/04/08/fighting-sliver-polygons-in-arcgis-thinness-ratio/
        /// </summary>
        /// <param name="closed2D">Closed 2D shape</param>
        /// <returns>ThinnessRatio</returns>
        public static double ThinnessRatio(this IClosed2D closed2D)
        {
            // Return NaN if the input is null
            if (closed2D == null)
                return double.NaN;

            // Create a temporary IClosed2D object
            IClosed2D closed2D_Temp = closed2D;

            // Check if the input object is of type Face, and if so, use its ExternalEdge2D property
            if (closed2D_Temp is Face)
                closed2D_Temp = ((Face)closed2D_Temp).ExternalEdge2D;

            // Check if the input object implements ICurvable2D
            if (closed2D_Temp is ICurvable2D)
            {
                // Calculate the area of the shape
                double area = closed2D_Temp.GetArea();

                // Return NaN if the area is NaN
                if (double.IsNaN(area))
                    return double.NaN;

                // Calculate the total length of the curves
                double length = 0;
                ((ICurvable2D)closed2D_Temp).GetCurves().ForEach(x => length += x.GetLength());

                // Calculate and return the Thinness Ratio using the area and length values
                return (4 * System.Math.PI * area) / (length * length);
            }

            // Throw a NotImplementedException if the input object does not implement ICurvable2D
            throw new System.NotImplementedException();
        }
    }
}