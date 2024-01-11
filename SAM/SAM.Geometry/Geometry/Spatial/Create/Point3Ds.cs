using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Point3D> Point3Ds(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<Point3D> result = new List<Point3D>();

            foreach (JObject jObject in jArray)
                result.Add(new Point3D(jObject));

            return result;
        }

        public static List<Point3D> Point3Ds(this BoundingBox3D boundingBox3D, double offset)
        {
            List<Point3D> result = new List<Point3D>();

            double width = boundingBox3D.Width;
            double height = boundingBox3D.Height;
            double depth = boundingBox3D.Depth;

            double distance_Width = 0;
            while (distance_Width <= width)
            {
                double distance_Height = 0;
                while (distance_Height <= height)
                {
                    double distance_Depth = 0;
                    while (distance_Depth <= depth)
                    {
                        result.Add(new Point3D(boundingBox3D.Min.X + distance_Width, boundingBox3D.Min.Y + distance_Depth, boundingBox3D.Min.Z + distance_Height));
                        distance_Depth += offset;
                    }
                    distance_Height += offset;
                }
                distance_Width += offset;
            }

            return result;
        }

        public static List<Point3D> Point3Ds(this IEnumerable<Segment3D> segment3Ds, bool close = false)
        {
            if (segment3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>() { segment3Ds.First().GetStart() };
            foreach (Segment3D segment3D in segment3Ds)
                result.Add(segment3D.GetEnd());

            if (close && result.First().Distance(result.Last()) != 0)
                result.Add(result.First());

            return result;
        }
    }
}