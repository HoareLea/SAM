using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Aperture Fit(this Aperture aperture, Face3D face3D, double areaFactor = 0.5, double offset = 0, double tolerance = Core.Tolerance.Distance)
        {
            if (aperture == null || face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face3D face3D_Aperture = aperture.GetFace3D();
            if(face3D_Aperture == null)
            {
                return null;
            }

            Geometry.Planar.Face2D face2D = plane.Convert(face3D);
            Geometry.Planar.Face2D face2D_Aperture = plane.Convert(face3D_Aperture);

            Geometry.Planar.ISegmentable2D segmentable2D = face2D_Aperture.ExternalEdge2D as Geometry.Planar.ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            double area = face2D_Aperture.GetArea();

            List<Geometry.Planar.Face2D> face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Aperture, face2D, tolerance);
            if(face2Ds_Difference == null || face2Ds_Difference.Count == 0)
            {
                return null;
            }

            double area_Difference = face2Ds_Difference.ConvertAll(x => x.GetArea()).Sum();
            if(area_Difference <= tolerance || area_Difference >= (areaFactor * area))
            {
                return null;
            }

            Geometry.Planar.BoundingBox2D boundingBox2D = face2D_Aperture.GetBoundingBox();

            Geometry.Planar.Vector2D vector2D = null;
            Geometry.Planar.Vector2D vector2D_X = null;
            Geometry.Planar.Vector2D vector2D_Y = null;
            foreach (Geometry.Planar.Point2D point2D in boundingBox2D.GetPoints())
            {
                if(face2D.Inside(point2D, tolerance))
                {
                    continue;
                }

                vector2D = Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldX, segmentable2D);
                if (vector2D != null && vector2D.Length > tolerance && (vector2D_X == null || vector2D.Length > vector2D_X.Length))
                {
                    vector2D_X = vector2D;
                }

                vector2D = Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldY, segmentable2D);
                if (vector2D != null && vector2D.Length > tolerance && (vector2D_Y == null || vector2D.Length > vector2D_Y.Length))
                {
                    vector2D_Y = vector2D;
                }
            }

            if(vector2D_X == null && vector2D_Y == null)
            {
                return null;
            }

            vector2D = new Geometry.Planar.Vector2D(vector2D_X.X, vector2D_Y.Y);
            if(vector2D.Length <= tolerance)
            {
                return null;
            }

            face2D_Aperture = Geometry.Planar.Query.Move(face2D_Aperture, vector2D);

            face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Aperture, face2D, tolerance);
            if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
            {
                return new Aperture(aperture.ApertureConstruction, plane.Convert(face2D_Aperture));
            }

            double area_Difference_New = face2Ds_Difference.ConvertAll(x => x.GetArea()).Sum();
            if (area_Difference_New <= tolerance)
            {
                return new Aperture(aperture.ApertureConstruction, plane.Convert(face2D_Aperture));
            }

            return null;

        }
    }
}