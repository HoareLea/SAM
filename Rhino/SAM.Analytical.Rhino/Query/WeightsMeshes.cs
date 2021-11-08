using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Query
    {
        public static List<Mesh> WeightsMeshes(this IEnumerable<Panel> panels, out List<Point3d> point3ds, out List<double> values, double offset = 0.2, double radiusFactor = 0.1)
        {
            point3ds = null;
            values = null;
            if (panels == null)
            {
                return null;
            }

            List<Mesh> result = new List<Mesh>();
            point3ds = new List<Point3d>();
            values = new List<double>();
            foreach(Panel panel in panels)
            {
                if(panel == null || !panel.TryGetValue(PanelParameter.Weight, out double weight))
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

                Point3d point3d = Geometry.Rhino.Convert.ToRhino(segment3D.Mid());

                Circle circle = new Circle(point3d, weight + radiusFactor);

                Mesh mesh = Mesh.CreateFromPlanarBoundary(circle.ToNurbsCurve(), MeshingParameters.Default, Core.Tolerance.MacroDistance);
                if (mesh == null)
                {
                    result.Add(null);
                    continue;
                }

                foreach(Point3f point3f in mesh.Vertices)
                {
                    mesh.VertexColors.Add(System.Drawing.Color.Black);
                }

                result.Add(mesh);
                point3ds.Add(point3d);
                values.Add(weight);
            }

            return result;
        }
    }
}