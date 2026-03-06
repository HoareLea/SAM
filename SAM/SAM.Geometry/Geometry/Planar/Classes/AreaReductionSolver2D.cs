using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class AreaReductionSolver2D
    {
        /// <summary>
        /// Angle tolerance in radians (used for rectangle detection).
        /// </summary>
        public double ToleranceAngleRad { get; set; } = Core.Tolerance.Angle;

        /// <summary>
        /// Distance tolerance (used for point closure, segment length checks, and offset convergence).
        /// </summary>
        public double ToleranceDistance { get; set; } = Core.Tolerance.MicroDistance;

        public bool IncludeExternalEdge { get; set; } = true;
        public bool IncludeInternalEdges { get; set; } = false;

        // Safety constants
        private const double EPS = 1e-12;

        /// <summary>
        /// Solves Face2D by shrinking to a given area reduction percent and returns offset faces.
        /// targetReductionPercent in [0..100].
        /// </summary>
        public List<Face2D> Solve(Face2D face2D, double targetReductionPercent)
        {
            if (face2D is null) return null;

            // Consistent bounds handling
            if (targetReductionPercent <= 0) return new List<Face2D>() { new Face2D(face2D.ToJObject()) };
            if (targetReductionPercent >= 100) return null;

            double offset = Solve_Offset(face2D, targetReductionPercent);
            if (double.IsNaN(offset)) return null;

            if (offset <= ToleranceDistance)
                return new List<Face2D>() { new Face2D(face2D.ToJObject()) };

            return face2D.Offset(-offset, IncludeExternalEdge, IncludeInternalEdges);
        }

        /// <summary>
        /// Solves Face2D by shrinking to a given area reduction percent and returns offset distance (positive).
        /// targetReductionPercent in (0..100).
        /// </summary>
        public double Solve_Offset(Face2D face2D, double targetReductionPercent)
        {
            if (face2D is null) return double.NaN;

            // Consistent bounds handling
            if (targetReductionPercent <= 0) return 0.0;
            if (targetReductionPercent >= 100) return double.NaN;

            // 1) Rectangle fast-path
            if (TryGetRectangleDimensions(face2D, out double width, out double height))
            {
                // Swap so width >= height (not required, but makes reasoning & clamps clearer)
                if (width < height)
                {
                    double tmp = width; width = height; height = tmp;
                }

                double t = targetReductionPercent / 100.0; // reduction fraction

                // Closed-form for rectangle:
                // 4 d^2 - 2(W+H)d + tWH = 0
                // d = ((W+H) - sqrt((W+H)^2 - 4 t W H))/4
                double S = width + height;
                double D = (S * S) - (4.0 * t * width * height);

                // Clamp for numeric noise
                if (D < 0 && D > -EPS) D = 0;
                if (D < 0) return double.NaN;

                double sqrtD = System.Math.Sqrt(D);
                double d = (S - sqrtD) / 4.0;

                // Feasibility
                double dMax = 0.5 * System.Math.Min(width, height);
                if (d < -EPS) return double.NaN;
                if (d < 0) d = 0; // tiny negative from floating point
                if (d - dMax > ToleranceDistance) return double.NaN;

                return d;
            }

            // 2) Fallback to numeric solver
            return SolveOffsetForAreaReduction(face2D, targetReductionPercent);
        }

        private double SolveOffsetForAreaReduction(Face2D face2D, double targetReductionPercent)
        {
            double A0 = face2D.GetArea();
            if (A0 <= 0) return double.NaN;

            double t = targetReductionPercent / 100.0;

            double AreaAfter(double d)
            {
                var faces = face2D.Offset(-d, IncludeExternalEdge, IncludeInternalEdges);
                if (faces == null || faces.Count == 0) return double.NaN;
                return faces.Sum(x => x.GetArea());
            }

            // Root form: f(d)=0
            double f(double d)
            {
                double At = AreaAfter(d);
                if (double.IsNaN(At)) return double.NaN;
                return (A0 - At) / A0 - t;
            }

            // Bracket
            double lo = 0.0;

            // Start scale: use minDimension if available, but keep a safe floor (important for “tiny segments” cases).
            // This avoids starting ridiculously small when geometry is over-segmented but overall size is not.
            double hi = GetInitialHi(face2D, A0);

            double flo = f(lo); // should be -t
            if (double.IsNaN(flo)) return double.NaN;

            double fhi = f(hi);
            int guard = 0;

            // Expand hi until we bracket or collapse
            while (!double.IsNaN(fhi) && fhi < 0.0 && guard++ < 60)
            {
                hi *= 2.0;
                fhi = f(hi);
            }

            if (double.IsNaN(fhi))
            {
                // collapsed before reaching target reduction
                return double.NaN;
            }

            // Bisection with better stopping condition:
            // 1) area-fraction residual small
            // 2) or interval in distance is small
            const double fracTol = 1e-4;
            for (int i = 0; i < 80; i++)
            {
                double mid = 0.5 * (lo + hi);

                // Distance convergence
                if ((hi - lo) <= ToleranceDistance)
                {
                    return mid;
                }

                double fmid = f(mid);
                if (double.IsNaN(fmid))
                {
                    // Mid collapsed; move upper bound down
                    hi = mid;
                    continue;
                }

                if (System.Math.Abs(fmid) < fracTol)
                {
                    return mid;
                }

                if (fmid < 0) lo = mid;
                else hi = mid;
            }

            return 0.5 * (lo + hi);
        }

        /// <summary>
        /// Initial bracket size: prefer geometric scale if available, with a robust floor and fallback.
        /// This avoids unsafe tiny hi when geometry is heavily segmented.
        /// </summary>
        private double GetInitialHi(Face2D face2D, double area)
        {
            // Option A: if you have a bounding box / extents in SAM, use it here:
            // var bbox = face2D.GetBoundingBox(); // (pseudo)
            // double minDim = Math.Min(bbox.Width, bbox.Height);

            // Option B (robust without bbox): scale from area, then clamp with a floor.
            // sqrt(area) is a characteristic length scale.
            double scale = System.Math.Sqrt(System.Math.Max(area, 0.0));

            // 1% of characteristic size, but never below a small floor and never below tolerance.
            double hi = 0.01 * scale;

            // Floor: ensures we don’t start extremely tiny.
            // You can tune this; using 1e-3 is often “1mm” if your units are meters.
            const double floor = 1e-3;

            hi = System.Math.Max(hi, floor);
            hi = System.Math.Max(hi, 10.0 * ToleranceDistance);

            return hi;
        }

        private bool TryGetRectangleDimensions(Face2D face2D, out double width, out double height)
        {
            width = double.NaN;
            height = double.NaN;

            // Require no holes for fast-path (keeps it unambiguous)
            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0) return false;

            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if (externalEdge == null) return false;

            IList<Point2D> pts = (externalEdge as ISegmentable2D)?.GetPoints();
            if (pts is null || pts.Count == 0) return false;

            // remove duplicated last point if closed explicitly
            if (pts.Count >= 2 && pts[0].Distance(pts[pts.Count - 1]) <= ToleranceDistance)
            {
                pts = pts.Take(pts.Count - 1).ToList();
            }

            if (pts.Count != 4) return false;

            var p0 = pts[0]; var p1 = pts[1]; var p2 = pts[2]; var p3 = pts[3];

            var e0 = p1 - p0;
            var e1 = p2 - p1;
            var e2 = p3 - p2;
            var e3 = p0 - p3;

            double l0 = e0.Length; double l1 = e1.Length; double l2 = e2.Length; double l3 = e3.Length;
            if (l0 <= ToleranceDistance || l1 <= ToleranceDistance || l2 <= ToleranceDistance || l3 <= ToleranceDistance)
                return false;

            var n0 = e0 / l0; var n1 = e1 / l1; var n2 = e2 / l2; var n3 = e3 / l3;

            // Convert angular tolerance (radians) into dot/cross thresholds (dimensionless)
            // Near 90°: |dot| <= sin(delta)
            // Near 0°/180°: |cross| <= sin(delta)
            double s = System.Math.Sin(System.Math.Max(ToleranceAngleRad, 0.0));

            // orthogonality: adjacent edges (dot near 0)
            if (System.Math.Abs(Dot(n0, n1)) > s) return false;
            if (System.Math.Abs(Dot(n1, n2)) > s) return false;
            if (System.Math.Abs(Dot(n2, n3)) > s) return false;
            if (System.Math.Abs(Dot(n3, n0)) > s) return false;

            // parallelism: opposite edges (cross near 0)
            if (System.Math.Abs(Cross2D(n0, n2)) > s) return false;
            if (System.Math.Abs(Cross2D(n1, n3)) > s) return false;

            // Dimensions: average opposite pairs for stability
            double w = 0.5 * (l0 + l2);
            double h = 0.5 * (l1 + l3);

            // Sanity: must have non-trivial area
            double area = externalEdge.GetArea();
            if (area <= ToleranceDistance) return false;

            width = w;
            height = h;
            return true;
        }

        private static double Dot(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;
        private static double Cross2D(Vector2D a, Vector2D b) => a.X * b.Y - a.Y * b.X;
    }
}