using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Point2D> Point2Ds(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<Point2D> result = new List<Point2D>();

            foreach (JObject jObject in jArray)
                result.Add(new Point2D(jObject));

            return result;
        }

        public static List<Point2D> Point2Ds(this BoundingBox2D boundingBox2D, int count)
        {
            if (count == -1)
                return null;

            return Point2Ds(boundingBox2D.Min.X, boundingBox2D.Min.Y, boundingBox2D.Max.X, boundingBox2D.Max.Y, count);
        }

        public static List<Point2D> Point2Ds(double x_min, double y_min, double x_max, double y_max, int count)
        {
            if (count == -1)
                return null;

            Random random = new Random();

            List<Point2D> result = new List<Point2D>();
            for (int i = 0; i < count; i++)
            {
                double x = Core.Query.NextDouble(random, x_min, x_max);
                double y = Core.Query.NextDouble(random, y_min, y_max);

                result.Add(new Point2D(x, y));
            }

            return result;
        }
    }
}