using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace SAM.Analytical.Classes
{
    public static class PolygonLabelCoordinate
    {
        /// <summary>
        /// Finds the center of the largest axis-aligned rectangle of fixed aspect ratio inside a room polygon.
        /// Returns the rectangle center and its width/height. Units follow the polygon (e.g., meters).
        /// </summary>
        /// <param name="roomIn">Room polygon (may have holes). Must be valid and simple.</param>
        /// <param name="aspect">Width / Height ratio. For H:W = 1:2 use aspect = 2.0.</param>
        /// <param name="clearance">Optional clearance to walls (euclidean). E.g., 0.15 = 150 mm.</param>
        /// <param name="gridFrac">Coarse grid step as fraction of min(room width,height). 0.05 is usually fine.</param>
        /// <param name="tolerance">Target geometric tolerance for sizes/steps.</param>
        /// <param name="maxIters">Max hill-climb iterations.</param>
        public static (Coordinate center, double width, double height) LargestAxisAlignedRectangle(
            Polygon roomIn,
            double aspect = 2.0,
            double clearance = 0.0,
            double gridFrac = 0.05,
            double tolerance = 0.005,
            int maxIters = 120)
        {
            if (roomIn == null) throw new ArgumentNullException(nameof(roomIn));
            if (aspect <= 0) throw new ArgumentOutOfRangeException(nameof(aspect));
            if (gridFrac <= 0 || gridFrac > 0.5) throw new ArgumentOutOfRangeException(nameof(gridFrac));
            if (tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));

            // Apply clearance (shrink). If the shrink empties the room, fall back to original.
            NetTopologySuite.Geometries.Geometry effective = clearance > 0 ? roomIn.Buffer(-clearance) : roomIn;
            if (effective.IsEmpty) effective = roomIn;

            // Work with the largest polygon piece if buffering split the space.
            var room = LargestPolygon(effective) ?? roomIn;
            var prep = PreparedGeometryFactory.Prepare(room);

            var env = room.EnvelopeInternal;
            double minSpan = System.Math.Min(env.Width, env.Height);
            double step = System.Math.Max(tolerance * 10.0, gridFrac * minSpan);

            // Seed from an interior point (fast and safe).
            Coordinate bestC = room.InteriorPoint.Coordinate;
            double bestS = MaxHalfWidthAt(prep, bestC, aspect, tolerance, step);

            // Coarse grid search to avoid local traps (handles L/S shapes).
            for (double x = env.MinX + step * 0.5; x <= env.MaxX - step * 0.5; x += step)
            {
                for (double y = env.MinY + step * 0.5; y <= env.MaxY - step * 0.5; y += step)
                {
                    var c = new Coordinate(x, y);
                    if (!prep.Covers(new Point(c))) continue;
                    double s = MaxHalfWidthAt(prep, c, aspect, tolerance, step);
                    if (s > bestS)
                    {
                        bestS = s;
                        bestC = c;
                    }
                }
            }

            // Hill climb / local refinement
            double delta = step;
            int iter = 0;
            while (delta > tolerance && iter++ < maxIters)
            {
                bool improved = false;

                foreach (var dir in Directions8)
                {
                    var c2 = new Coordinate(bestC.X + delta * dir.dx, bestC.Y + delta * dir.dy);
                    if (!prep.Covers(new Point(c2))) continue;

                    double s2 = MaxHalfWidthAt(prep, c2, aspect, tolerance, step);
                    if (s2 > bestS + 1e-9)
                    {
                        bestS = s2;
                        bestC = c2;
                        improved = true;
                        break; // greedy accept; restart neighborhood
                    }
                }

                if (!improved) delta *= 0.5;
            }

            // Convert half-width to rectangle dims (axis-aligned)
            double halfW = bestS;
            double halfH = bestS / aspect;
            double width = 2.0 * halfW;
            double height = 2.0 * halfH;

            return (bestC, width, height);
        }

        // --- Helpers ---

        private static (double dx, double dy)[] Directions8 = new (double, double)[]
        {
        (1,0), (-1,0), (0,1), (0,-1),
        ( 0.70710678,  0.70710678), ( 0.70710678, -0.70710678),
        (-0.70710678,  0.70710678), (-0.70710678, -0.70710678)
        };

        /// <summary>
        /// For a given center, finds the maximum half-width (along X) such that a rectangle
        /// of aspect = width/height fits entirely inside the prepared polygon. Binary search.
        /// </summary>
        private static double MaxHalfWidthAt(IPreparedGeometry prep, Coordinate center, double aspect, double tol, double seed)
        {
            // Grow-to-fail to get an upper bound
            double low = 0.0;
            double high = System.Math.Max(seed, tol);
            for (int i = 0; i < 32; i++)
            {
                if (Fits(prep, center, high, aspect)) { low = high; high *= 2.0; }
                else break;
            }

            // If even tiny rectangle doesn't fit, return 0
            if (low == 0.0 && !Fits(prep, center, tol, aspect)) return 0.0;

            // Binary search to tolerance
            for (int i = 0; i < 40; i++)
            {
                double mid = 0.5 * (low + high);
                if (high - low <= tol) break;
                if (Fits(prep, center, mid, aspect)) low = mid; else high = mid;
            }
            return low;
        }

        /// <summary>
        /// Checks if an axis-aligned rectangle centered at 'c' with half-width 'halfW'
        /// and aspect = width/height fits inside the prepared polygon.
        /// </summary>
        private static bool Fits(IPreparedGeometry prep, Coordinate c, double halfW, double aspect)
        {
            if (halfW <= 0) return false;
            double halfH = halfW / aspect;

            var ring = new LinearRing(new[]
            {
            new Coordinate(c.X - halfW, c.Y - halfH),
            new Coordinate(c.X + halfW, c.Y - halfH),
            new Coordinate(c.X + halfW, c.Y + halfH),
            new Coordinate(c.X - halfW, c.Y + halfH),
            new Coordinate(c.X - halfW, c.Y - halfH)
        });

            var rect = new Polygon(ring);

            // Covers allows touching boundary; if you want strict inside, use Contains.
            return prep.Covers(rect);
        }

        private static Polygon LargestPolygon(NetTopologySuite.Geometries.Geometry g)
        {
            if (g is Polygon p) return p;

            if (g is MultiPolygon mp)
            {
                double bestA = double.NegativeInfinity;
                Polygon best = null;
                for (int i = 0; i < mp.NumGeometries; i++)
                {
                    var pi = (Polygon)mp.GetGeometryN(i);
                    double a = pi.Area;
                    if (a > bestA) { bestA = a; best = pi; }
                }
                return best;
            }
            // If it's something else (e.g., empty), give up.
            return null;
        }
    }
}
