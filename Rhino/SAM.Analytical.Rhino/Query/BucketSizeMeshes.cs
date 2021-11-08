using Rhino.Geometry;
using Rhino.Geometry.Collections;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Query
    {
        public static List<Mesh> BucketSizeMeshes(this IEnumerable<Panel> panels, out List<Point3d> point3Ds, out List<double> values, double offset = 0.1)
        {
            point3Ds = null;
            values = null;

            if(panels == null)
            {
                return null;
            }

            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (Panel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                if (!panel.TryGetValue(PanelParameter.BucketSize, out double bucketSize))
                {
                    continue;
                }

                if(bucketSize < min)
                {
                    min = bucketSize;
                }

                if(bucketSize > max)
                {
                    max = bucketSize;
                }
            }

            if(min == double.MaxValue || max == double.MinValue)
            {
                return null;
            }

            List<Mesh> result = new List<Mesh>();
            point3Ds = new List<Point3d>();
            values = new List<double>();
            foreach(Panel panel in panels)
            {
                if(panel == null || !panel.TryGetValue(PanelParameter.BucketSize, out double bucketSize))
                {
                    result.Add(null);
                    continue;
                }

                Face3D face3D = panel.GetFace3D();
                if(face3D == null)
                {
                    result.Add(null);
                    continue;
                }

                Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(face3D.GetBoundingBox().Min.Z + offset);

                Segment3D segment3D = Geometry.Spatial.Query.MaxIntersectionSegment3D(plane, face3D);
                if(segment3D == null)
                {
                    result.Add(null);
                    continue;
                }

                Line line = Geometry.Rhino.Convert.ToRhino_Line(segment3D);

                Vector3d axis_1 = line.UnitTangent;
                Vector3d axis_2 = Vector3d.CrossProduct(axis_1, Vector3d.ZAxis);

                global::Rhino.Geometry.Plane plane_1 = new global::Rhino.Geometry.Plane(line.PointAt(0.5), axis_2, axis_1);

                Rectangle3d rectangle3d = new Rectangle3d(plane_1, new Interval(-bucketSize, bucketSize), new Interval(-line.Length / 2, line.Length / 2));

                Polyline polyline = rectangle3d.ToPolyline();
                if(polyline == null)
                {
                    result.Add(null);
                    continue;
                }

                Mesh mesh = Mesh.CreateFromClosedPolyline(polyline);
                if (mesh == null)
                {
                    result.Add(null);
                    continue;
                }

                int red = 0;
                int green = 0;
                int blue = 0;
                if (bucketSize < (min + max) / 2)
                {
                    green = 255;
                    double value = System.Math.Round(255 * (2 * ((bucketSize - min) / (max - min))));
                    red = double.IsNaN(value) ? 128 : System.Convert.ToInt32(value);
                }
                else
                {
                    double value = System.Math.Round(255 * (2 * (1 - ((bucketSize - min) / (max - min)))));
                    green = double.IsNaN(value) ? 128 : System.Convert.ToInt32(value);
                    red = 255;
                }

                MeshVertexList meshVertexList = mesh.Vertices;
                foreach(Point3f point3f in meshVertexList)
                {
                    mesh.VertexColors.Add(red, green, blue);
                }

                result.Add(mesh);
                point3Ds.Add(rectangle3d.Center);
                values.Add(bucketSize);
            }

            return result;
        }
    }
}