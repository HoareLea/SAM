using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static void ViewField<T>(
            this IEnumerable<T> face3DObjects,
            Vector3D viewDirection,
            out List<LinkedFace3D> linkedFace3Ds_Hidden,
            out List<LinkedFace3D> linkedFace3Ds_Visible,
            bool hidden = true,
            bool visible = true,
            double tolerance_Area = Tolerance.MacroDistance,
            double tolerance_Snap = Tolerance.MacroDistance,
            double tolerance_Angle = Tolerance.Angle,
            double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        {
            var hiddenFaces = hidden ? new ConcurrentBag<LinkedFace3D>() : null;
            var visibleFaces = visible ? new ConcurrentBag<LinkedFace3D>() : null;

            linkedFace3Ds_Hidden = hidden ? new List<LinkedFace3D>() : null;
            linkedFace3Ds_Visible = visible ? new List<LinkedFace3D>() : null;

            if (face3DObjects == null || viewDirection == null || !viewDirection.IsValid())
            {
                return;
            }

            var linkedFace3Ds = new ConcurrentBag<LinkedFace3D>(face3DObjects
                .Select(face3DObject => face3DObject is LinkedFace3D linked ? linked : Create.LinkedFace3D(face3DObject))
                .Where(linkedFace3D => linkedFace3D != null));

            if (linkedFace3Ds.Count < 2)
            {
                if (visible)
                {
                    linkedFace3Ds_Visible.AddRange(linkedFace3Ds);
                }
                return;
            }

            BoundingBox3D boundingBox3D = Create.BoundingBox3D(linkedFace3Ds);
            if (boundingBox3D == null || !boundingBox3D.IsValid())
            {
                return;
            }

            Vector3D viewVector = new Vector3D(viewDirection).Unit * boundingBox3D.Min.Distance(boundingBox3D.Max);
            Point3D viewPoint = boundingBox3D.GetCentroid().GetMoved(viewVector.GetNegated()) as Point3D;

            Plane projectionPlane = new Plane(viewPoint, viewVector.Unit);

            var projectedFaces = new ConcurrentBag<Tuple<LinkedFace3D, Geometry.Planar.Face2D>>(linkedFace3Ds
                .Select(linkedFace3D => new
                {
                    LinkedFace = linkedFace3D,
                    ProjectedFace = projectionPlane.Project(linkedFace3D.Face3D, viewVector, tolerance_Distance)
                })
                .Where(x => x.ProjectedFace != null && x.ProjectedFace.IsValid())
                .Select(x => new Tuple<LinkedFace3D, Geometry.Planar.Face2D>(
                    x.LinkedFace,
                    projectionPlane.Convert(x.ProjectedFace)))
                .Where(t => t.Item2.GetArea() >= tolerance_Area));

            // Adjust this part to retrieve segments correctly
            var segments = new ConcurrentBag<Geometry.Planar.Segment2D>(projectedFaces
                .SelectMany(t => t.Item2.Edge2Ds) // Assuming GetEdges() returns the edges of the Face2D
                .SelectMany(edge => ((ISegmentable2D)edge).GetSegments())); // Assuming each edge can provide segments

            segments = new ConcurrentBag<Geometry.Planar.Segment2D>(Geometry.Planar.Query.Split(segments.ToList(), tolerance_Distance));
            segments = new ConcurrentBag<Geometry.Planar.Segment2D>(Geometry.Planar.Query.Snap(segments.ToList(), true, tolerance_Snap));

            List<Geometry.Planar.Face2D> planarFaces = Geometry.Planar.Create.Face2Ds(segments.ToList(), EdgeOrientationMethod.Undefined, tolerance_Distance);

            if (planarFaces == null)
            {
                return;
            }

            List<Geometry.Planar.IClosed2D> holes = Geometry.Planar.Query.Holes(planarFaces);
            if (holes != null)
            {
                planarFaces.AddRange(holes.Select(hole => new Geometry.Planar.Face2D(hole)));
            }

            var faceWithPoints = new ConcurrentBag<Tuple<Geometry.Planar.Face2D, Geometry.Planar.Point2D>>(planarFaces
                .Select(face => new Tuple<Geometry.Planar.Face2D, Geometry.Planar.Point2D>(face, face.GetInternalPoint2D(tolerance_Distance))));

            Parallel.ForEach(projectedFaces, tuple =>
            {
                Plane facePlane = tuple.Item1.Face3D.GetPlane();
                if (facePlane == null)
                {
                    return;
                }

                Vector3D projectedVector = facePlane.Project(viewDirection);
                if (projectedVector != null && projectedVector.IsValid() && projectedVector.Length > tolerance_Distance)
                {
                    double angle = viewDirection.SmallestAngle(projectedVector);
                    if (angle < tolerance_Angle)
                    {
                        if (hidden)
                        {
                            hiddenFaces?.Add(tuple.Item1);
                        }
                        return;
                    }
                }

                foreach (var facePoint in faceWithPoints)
                {
                    if (!tuple.Item2.Inside(facePoint.Item2, tolerance_Distance))
                    {
                        continue;
                    }

                    Point3D startPoint = projectionPlane.Convert(facePoint.Item2);
                    Point3D endPoint = startPoint.GetMoved(viewVector * 2) as Point3D;
                    Segment3D segment = new Segment3D(startPoint, endPoint);

                    Dictionary<LinkedFace3D, Point3D> intersections = Query.IntersectionDictionary(segment, linkedFace3Ds.ToList(), true, tolerance_Distance);
                    if (intersections == null || intersections.Count == 0)
                    {
                        continue;
                    }

                    bool isVisible = intersections.Keys.First() == tuple.Item1;
                    if (isVisible && visible)
                    {
                        visibleFaces?.Add(tuple.Item1);
                    }
                    else if (!isVisible && hidden)
                    {
                        hiddenFaces?.Add(tuple.Item1);
                    }
                }
            });

            if (hiddenFaces != null)
            {
                linkedFace3Ds_Hidden.AddRange(hiddenFaces);
            }

            if (visibleFaces != null)
            {
                linkedFace3Ds_Visible.AddRange(visibleFaces);
            }
        }
    }
}
