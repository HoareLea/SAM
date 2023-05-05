using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// A static class that contains methods to create 2D geometries.
    /// </summary>
    public static partial class Create
    {
        /// <summary>
        /// Returns a closed 2D geometry from the specified JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object.</param>
        /// <returns>A closed 2D geometry.</returns>
        public static IClosed2D IClosed2D(this JObject jObject)
        {
            if (jObject == null)
                return null;

            return Geometry.Create.ISAMGeometry(jObject) as IClosed2D;
        }
        /// <summary>
        /// Returns a closed 2D geometry from the specified 2D polygon, with optional tolerance.
        /// </summary>
        /// <param name="polygon2D">The 2D polygon.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>A closed 2D geometry.</returns>
        public static IClosed2D IClosed2D(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            // Get the points of the polygon
            List<Point2D> point2Ds = polygon2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count < 3)
                return null;

            // If the polygon has 3 points, it's a triangle
            if (point2Ds.Count == 3)
                return new Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);

            // If the polygon is rectangular, it's a rectangle
            if (polygon2D.Rectangular(tolerance))
                return Rectangle2D(point2Ds);

            // Otherwise, it's a general polygon
            return new Polygon2D(point2Ds);
        }

        /// <summary>
        /// Returns a closed 2D geometry from the specified 2D polycurve, with optional tolerance.
        /// </summary>
        /// <param name="polycurve2D">The 2D polycurve.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>A closed 2D geometry.</returns>
        public static IClosed2D IClosed2D(this Polycurve2D polycurve2D, double tolerance = Core.Tolerance.Distance)
        {
            if(polycurve2D == null)
            {
                return null;
            }

            // This code is not implemented yet
            throw new System.NotImplementedException();

            if(polycurve2D.GetCurves().TrueForAll(x => x is Segment2D))
            {
                //TO be implemented
            }

        }

        /// <summary>
        /// Returns a closed 2D geometry from the specified closed 2D geometry, with optional tolerance.
        /// </summary>
        /// <param name="closed2D">The closed 2D geometry.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>A closed 2D geometry.</returns>
        public static IClosed2D IClosed2D(this IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if(closed2D == null)
            {
                return null;
            }

            if(closed2D is Polygon2D)
            {
                return IClosed2D((Polygon2D)closed2D, tolerance);
            }

            if (closed2D is Face2D)
            {
                // If the closed 2D geometry is a face, create a new closed 2D geometry from it
                return new Face2D((Face2D)closed2D);
            }

            if (closed2D is Polycurve2D)
            {
                // If the closed 2D geometry is a polycurve, create a new closed 2D geometry from it
                return IClosed2D((Polycurve2D)closed2D, tolerance);
            }

            // If the closed 2D geometry is of an unknown type, throw an exception
            throw new System.NotImplementedException();
        }
    }
}