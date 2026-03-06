using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class AreaReductionSolver2D
    {
        public double tolerance_Angle { get; set; } = Core.Tolerance.Angle;
        public double tolerance_ParallelCross { get; set; } = Core.Tolerance.MacroDistance;
        public double tolerance { get; set; } = Core.Tolerance.MicroDistance;
        public bool IncludeExternalEdge { get; set; } = true;
        public bool IncludeInternalEdges { get; set; } = false;

        /// <summary>
        /// Solves Face2Ds by shrinking to given area and returns face2Ds
        /// </summary>
        /// <param name="face2D">Face2D</param>
        /// <param name="target">Target percent of area [0-100]</param>
        /// <returns>Face2Ds</returns>
        public List<Face2D> Solve(Face2D face2D, double target)
        {
            if(face2D is null)
            {
                return null;
            }

            if(target == 0)
            {
                return []; 
            }

            if(target == 100)
            {
                return new List<Face2D>() { new Face2D(face2D.ToJObject()) };
            }

            double offset = Solve_Offset(face2D, target);
            if(double.IsNaN(offset))
            {
                return null;
            }

            if(offset == 0)
            {
                return new List<Face2D>() { new Face2D(face2D.ToJObject()) };
            }

            return face2D.Offset(-offset, IncludeExternalEdge, IncludeInternalEdges);
        }

        /// <summary>
        /// Solves Face2Ds by shrinking to given area and returns offset
        /// </summary>
        /// <param name="face2D">Face2D</param>
        /// <param name="target">Target percent of area [0-100]</param>
        /// <returns>offset (positive!)</returns>
        public double Solve_Offset(Face2D face2D, double target)
        {
            if (face2D == null)
            {
                return double.NaN;
            }

            if (target <= 0)
            {
                return 0.0;
            }
            if (target >= 100)
            {
                return double.NaN;
            }

            // 1) Try rectangle fast-path
            if (TryGetRectangleDimensions(face2D, out double width, out double height))
            {
                double t = target / 100.0;

                // Quadratic closed-form:
                double S = width + height;
                double D = S * S - 4.0 * t * width * height;
                if (D < 0) 
                {
                    D = 0; // guard for numeric noise
                }

                double d = (S - System.Math.Sqrt(D)) / 4.0;

                // ensure feasible
                double dMax = 0.5 * System.Math.Min(width, height);
                if (d < 0)
                {
                    return double.NaN;
                }

                if (d > dMax)
                {
                    return double.NaN;
                }

                return d;
            }

            // 2) Fallback to generic numeric solver (your existing approach)
            return SolveOffsetForAreaReduction(face2D, target); // your iterative solver
        }

        private double SolveOffsetForAreaReduction(Face2D face2D, double target)
        {
            if (face2D == null)
            {
                return double.NaN;
            }

            double A0 = face2D.GetArea();
            if (A0 <= 0)
            {
                return double.NaN;
            }

            double t = target / 100.0;

            double AreaAfter(double d)
            {
                // inward shrink => negative offset
                var faces = face2D.Offset(-d, IncludeExternalEdge, IncludeInternalEdges);
                if (faces == null || faces.Count == 0)
                {
                    return double.NaN;
                }

                return faces.Sum(x => x.GetArea());
            }

            double f(double d)
            {
                double At = AreaAfter(d);
                if (double.IsNaN(At))
                {
                    return double.NaN;
                }

                return (A0 - At) / A0 - t;
            }

            // bracket
            double lo = 0.0;
            double hi = 0.01; // start small (1cm) – or derive from bbox
            double flo = f(lo); // should be -t
            if (double.IsNaN(flo))
            {
                return double.NaN;
            }

            double fhi = f(hi);
            int guard = 0;
            while (!double.IsNaN(fhi) && fhi < 0.0 && guard++ < 60)
            {
                hi *= 2.0;
                fhi = f(hi);
            }

            // if we collapsed before reaching target: no solution at requested % (too aggressive)
            if (double.IsNaN(fhi))
            {
                return double.NaN;
            }

            // bisection
            for (int i = 0; i < 80; i++)
            {
                double mid = 0.5 * (lo + hi);
                double fmid = f(mid);
                if (double.IsNaN(fmid)) 
                { 
                    hi = mid; continue; 
                }

                if (System.Math.Abs(fmid) < 1e-4)
                {
                    return mid;// tolerance on fraction
                }

                if (fmid < 0)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return 0.5 * (lo + hi);
        }

        private bool TryGetRectangleDimensions(Face2D face2D, out double width, out double height)
        {
            width = double.NaN;
            height = double.NaN;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                return false;
            }

            // If you want to ignore holes, just use ExternalEdge only.
            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if (externalEdge == null)
            {
                return false;
            }

            IList<Point2D> point2Ds = (externalEdge as ISegmentable2D)?.GetPoints();
            if(point2Ds is null || point2Ds.Count == 0)
            {
                return false;
            }

            if (point2Ds == null)
            {
                return false;
            }

            // remove duplicated last point if closed explicitly
            if (point2Ds.Count >= 2 && point2Ds[0].Distance(point2Ds[point2Ds.Count - 1]) <= tolerance)
            {
                point2Ds = point2Ds.Take(point2Ds.Count - 1).ToList();
            }

            if (point2Ds.Count != 4)
            {
                return false;
            }

            var p0 = point2Ds[0]; var p1 = point2Ds[1]; var p2 = point2Ds[2]; var p3 = point2Ds[3];

            var e0 = p1 - p0;
            var e1 = p2 - p1;
            var e2 = p3 - p2;
            var e3 = p0 - p3;

            double l0 = e0.Length; double l1 = e1.Length; double l2 = e2.Length; double l3 = e3.Length;
            if (l0 <= tolerance || l1 <= tolerance || l2 <= tolerance || l3 <= tolerance) return false;

            var n0 = e0 / l0; var n1 = e1 / l1; var n2 = e2 / l2; var n3 = e3 / l3;

            // orthogonality: adjacent edges
            if (System.Math.Abs(Dot(n0, n1)) > tolerance_Angle)
            {
                return false;
            }

            if (System.Math.Abs(Dot(n1, n2)) > tolerance_Angle)
            {
                return false;
            }

            if (System.Math.Abs(Dot(n2, n3)) > tolerance_Angle)
            {
                return false;
            }

            if (System.Math.Abs(Dot(n3, n0)) > tolerance_Angle)
            {
                return false;
            }

            // parallelism: opposite edges
            if (System.Math.Abs(Cross2D(n0, n2)) > tolerance_ParallelCross)
            {
                return false;
            }

            if (System.Math.Abs(Cross2D(n1, n3)) > tolerance_ParallelCross)
            {
                return false;
            }

            // Dimensions: pick adjacent lengths; average opposite pairs for stability
            double w = 0.5 * (l0 + l2);
            double h = 0.5 * (l1 + l3);

            // Optional: also verify area consistency
            double area = externalEdge.GetArea();
            if (area <= tolerance)
            {
                return false;
            }

            // set outputs
            width = w;
            height = h;
            return true;
        }

        // Helpers (replace Point2D ops with SAM equivalents)
        private static double Dot(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;
        
        private static double Cross2D(Vector2D a, Vector2D b) => a.X * b.Y - a.Y * b.X;
    }
}
