// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Point3D> Point3Ds(this JsonArray jsonArray)
        {
            if (jsonArray == null)
                return null;

            List<Point3D> result = new List<Point3D>();

            foreach (JsonNode jsonNode in jsonArray)
                result.Add(new Point3D(jsonNode as JsonObject));

            return result;
        }

        public static List<Point3D> Point3Ds(this BoundingBox3D boundingBox3D, double offset)
        {
            if (boundingBox3D == null)
                return null;

            if (offset <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than zero.");

            List<Point3D> result = new List<Point3D>();

            double width = boundingBox3D.Width;
            double height = boundingBox3D.Height;
            double depth = boundingBox3D.Depth;

            double minX = boundingBox3D.MinX;
            double minY = boundingBox3D.MinY;
            double minZ = boundingBox3D.MinZ;

            double distance_Width = 0;
            while (distance_Width <= width)
            {
                double distance_Height = 0;
                while (distance_Height <= height)
                {
                    double distance_Depth = 0;
                    while (distance_Depth <= depth)
                    {
                        result.Add(new Point3D(minX + distance_Width, minY + distance_Depth, minZ + distance_Height));
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
