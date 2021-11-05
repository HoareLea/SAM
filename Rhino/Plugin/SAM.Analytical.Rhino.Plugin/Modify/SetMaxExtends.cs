using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Rhino.Plugin
{
    public static partial class Modify
    {
        public static void SetMaxExtends(this List<Panel> panels, double offset = 0.1, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null)
            {
                return;
            }

            for(int i =0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if(panel == null)
                {
                    continue;
                }
                
                double thickness = double.NaN;

                Construction construction = panel.Construction;
                if (construction != null)
                {
                    thickness = construction.GetThickness();
                }
                else
                {
                    if (!construction.TryGetValue(ConstructionParameter.DefaultThickness, out thickness))
                    {
                        thickness = double.NaN;
                    }
                }

                double maxExtend = double.NaN;
                if(double.IsNaN(thickness))
                {
                    maxExtend = 0.33;
                }
                else if (thickness > 0.29)
                {
                    maxExtend = 0.6;
                }
                else
                {
                    maxExtend = 0.5;
                }

                double length = double.NaN;

                Face3D face3D = panel.GetFace3D();

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();

                if (boundingBox3D.Height > offset)
                {
                    Plane plane = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + offset);

                    PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
                    if (planarIntersectionResult != null && planarIntersectionResult.Intersecting)
                    {
                        List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();
                        if(segmentable3Ds != null)
                        {
                            List<Point3D> point3Ds = new List<Point3D>();
                            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                            {
                                List<Point3D> point3Ds_Temp = segmentable3D?.GetPoints();
                                if (point3Ds_Temp != null)
                                {
                                    point3Ds.AddRange(point3Ds_Temp);
                                }
                            }

                            point3Ds.ExtremePoints(out Point3D point3D_1, out Point3D point3D_2);
                            length = point3D_1.Distance(point3D_2);
                        }
                    }
                }
                else
                {
                    Plane plane = face3D.GetPlane();
                    if (plane == null)
                    {
                        continue;
                    }

                    Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D((plane.Convert(face3D).ExternalEdge2D as Geometry.Planar.ISegmentable2D)?.GetPoints());
                    if (rectangle2D == null)
                    {
                        continue;
                    }

                    length = System.Math.Max(rectangle2D.Height, rectangle2D.Width); 
                }

                if(double.IsNaN(length))
                {
                    length = 0;
                }

                length = 0.49 * length;

                maxExtend = System.Math.Min(length, maxExtend);

                panels[i].SetValue(PanelParameter.MaxExtend, maxExtend);
            }
        }
    }
}