// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static void SplitFace3Ds(this List<Shell> shells, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count == 0)
                return;

            int count = shells.Count;

            List<Shell> shells_Result = Enumerable.Repeat<Shell>(null, count).ToList();

            // One bounding box per shell instead of one per pair. Shell.GetBoundingBox()
            // allocates a copy on every call, so the old shells.FindAll(...) inside the
            // per-shell loop produced O(n^2) allocations before any geometry was touched.
            BoundingBox3D[] boundingBox3Ds = new BoundingBox3D[count];
            for (int i = 0; i < count; i++)
            {
                boundingBox3Ds[i] = shells[i]?.GetBoundingBox();
            }

            BoundingBox3DGrid grid = new BoundingBox3DGrid(boundingBox3Ds, tolerance_Distance);

            Parallel.For(0, count, (int i) =>
            //for(int i=0; i < count; i++)
            {
                Shell shell = shells[i];
                if (shell == null)
                {
                    return;
                }

                BoundingBox3D boundingBox3D = boundingBox3Ds[i];

                // Same predicate as before - reference inequality plus the exact InRange test -
                // just evaluated against the grid neighbourhood rather than the whole list, and
                // yielded in ascending index order so the sequence of splits is unchanged.
                List<Shell> shells_InRange = null;
                foreach (int j in grid.Candidates(boundingBox3D))
                {
                    Shell shell_Temp = shells[j];
                    if (shell_Temp == null || ReferenceEquals(shell_Temp, shell))
                    {
                        continue;
                    }

                    if (!boundingBox3D.InRange(boundingBox3Ds[j], tolerance_Distance))
                    {
                        continue;
                    }

                    if (shells_InRange == null)
                    {
                        shells_InRange = new List<Shell>();
                    }

                    shells_InRange.Add(shell_Temp);
                }

                if (shells_InRange == null || shells_InRange.Count == 0)
                {
                    shells_Result[i] = shell;
                    return;
                }

                shells_Result[i] = shell;

                shell = new Shell(shell);

                bool split = false;
                foreach (Shell shell_InRange in shells_InRange)
                {
                    if (shell.SplitFace3Ds(shell_InRange, tolerance_Snap, tolerance_Angle, tolerance_Distance))
                        split = true;
                }

                if (split)
                    shells_Result[i] = shell;

            });

            shells.Clear();
            shells.AddRange(shells_Result);
        }

        /// <summary>
        /// Uniform 3D hash over a fixed set of bounding boxes. Cell size is the median box
        /// extent so a typical building model puts a handful of boxes in each cell; boxes that
        /// would span an unreasonable number of cells are held aside and offered to every
        /// query instead, which keeps insertion bounded for outliers such as a site-sized box
        /// among room-sized ones. Candidates come back in ascending index order.
        /// </summary>
        private sealed class BoundingBox3DGrid
        {
            private const int MaxCellsPerItem = 4096;

            private readonly BoundingBox3D[] boundingBox3Ds;
            private readonly double cellSize;
            private readonly double tolerance;
            private readonly Dictionary<Tuple<long, long, long>, List<int>> dictionary;
            private readonly List<int> oversized;

            public BoundingBox3DGrid(BoundingBox3D[] boundingBox3Ds, double tolerance)
            {
                this.boundingBox3Ds = boundingBox3Ds;
                this.tolerance = tolerance > 0 ? tolerance : Core.Tolerance.Distance;

                dictionary = new Dictionary<Tuple<long, long, long>, List<int>>();
                oversized = new List<int>();

                cellSize = CellSize(boundingBox3Ds, this.tolerance);

                for (int i = 0; i < boundingBox3Ds.Length; i++)
                {
                    BoundingBox3D boundingBox3D = boundingBox3Ds[i];
                    if (boundingBox3D == null)
                    {
                        continue;
                    }

                    if (!IsFinite(boundingBox3D))
                    {
                        // NaN or infinite bounds cannot be quantised into cells - hold the box
                        // aside so the exact predicate still sees it, exactly as an oversized box.
                        oversized.Add(i);
                        continue;
                    }

                    Cells(boundingBox3D, out long kx1, out long kx2, out long ky1, out long ky2, out long kz1, out long kz2);

                    if (!IsValid(kx1, kx2, ky1, ky2, kz1, kz2) || CellCount(kx1, kx2, ky1, ky2, kz1, kz2) > MaxCellsPerItem)
                    {
                        oversized.Add(i);
                        continue;
                    }

                    for (long kx = kx1; kx <= kx2; kx++)
                    {
                        for (long ky = ky1; ky <= ky2; ky++)
                        {
                            for (long kz = kz1; kz <= kz2; kz++)
                            {
                                Tuple<long, long, long> key = new Tuple<long, long, long>(kx, ky, kz);
                                if (!dictionary.TryGetValue(key, out List<int> list))
                                {
                                    list = new List<int>();
                                    dictionary[key] = list;
                                }

                                list.Add(i);
                            }
                        }
                    }
                }
            }

            public List<int> Candidates(BoundingBox3D boundingBox3D)
            {
                if (boundingBox3D == null)
                {
                    return new List<int>();
                }

                HashSet<int> indexes = new HashSet<int>(oversized);

                if (!IsFinite(boundingBox3D))
                {
                    // The query box itself cannot be quantised - fall back to the full set.
                    for (int i = 0; i < boundingBox3Ds.Length; i++)
                    {
                        indexes.Add(i);
                    }
                }
                else
                {
                    Cells(boundingBox3D, out long kx1, out long kx2, out long ky1, out long ky2, out long kz1, out long kz2);

                    if (!IsValid(kx1, kx2, ky1, ky2, kz1, kz2) || CellCount(kx1, kx2, ky1, ky2, kz1, kz2) > MaxCellsPerItem)
                    {
                        // The query box itself is an outlier - fall back to the full set rather than
                        // walking millions of cells. Still correct, just not accelerated.
                        for (int i = 0; i < boundingBox3Ds.Length; i++)
                        {
                            indexes.Add(i);
                        }
                    }
                    else
                    {
                        for (long kx = kx1; kx <= kx2; kx++)
                        {
                            for (long ky = ky1; ky <= ky2; ky++)
                            {
                                for (long kz = kz1; kz <= kz2; kz++)
                                {
                                    if (dictionary.TryGetValue(new Tuple<long, long, long>(kx, ky, kz), out List<int> list))
                                    {
                                        foreach (int index in list)
                                        {
                                            indexes.Add(index);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                List<int> result = new List<int>(indexes);
                result.Sort();
                return result;
            }

            private static bool IsFinite(BoundingBox3D boundingBox3D)
            {
                Point3D min = boundingBox3D?.Min;
                Point3D max = boundingBox3D?.Max;

                if (min == null || max == null)
                {
                    return false;
                }

                return IsFinite(min.X) && IsFinite(min.Y) && IsFinite(min.Z) && IsFinite(max.X) && IsFinite(max.Y) && IsFinite(max.Z);
            }

            private static bool IsFinite(double value)
            {
                return !double.IsNaN(value) && !double.IsInfinity(value);
            }

            private static bool IsValid(long kx1, long kx2, long ky1, long ky2, long kz1, long kz2)
            {
                return kx2 >= kx1 && ky2 >= ky1 && kz2 >= kz1;
            }

            private static double CellCount(long kx1, long kx2, long ky1, long ky2, long kz1, long kz2)
            {
                // Double arithmetic on purpose: the long product can overflow before the guard sees it.
                return ((double)kx2 - kx1 + 1.0) * ((double)ky2 - ky1 + 1.0) * ((double)kz2 - kz1 + 1.0);
            }

            private void Cells(BoundingBox3D boundingBox3D, out long kx1, out long kx2, out long ky1, out long ky2, out long kz1, out long kz2)
            {
                Point3D min = boundingBox3D.Min;
                Point3D max = boundingBox3D.Max;

                kx1 = (long)System.Math.Floor((min.X - tolerance) / cellSize);
                kx2 = (long)System.Math.Floor((max.X + tolerance) / cellSize);
                ky1 = (long)System.Math.Floor((min.Y - tolerance) / cellSize);
                ky2 = (long)System.Math.Floor((max.Y + tolerance) / cellSize);
                kz1 = (long)System.Math.Floor((min.Z - tolerance) / cellSize);
                kz2 = (long)System.Math.Floor((max.Z + tolerance) / cellSize);
            }

            private static double CellSize(BoundingBox3D[] boundingBox3Ds, double tolerance)
            {
                List<double> extents = new List<double>();
                foreach (BoundingBox3D boundingBox3D in boundingBox3Ds)
                {
                    if (!IsFinite(boundingBox3D))
                    {
                        continue;
                    }

                    Point3D min = boundingBox3D.Min;
                    Point3D max = boundingBox3D.Max;

                    extents.Add(System.Math.Max(max.X - min.X, System.Math.Max(max.Y - min.Y, max.Z - min.Z)));
                }

                if (extents.Count == 0)
                {
                    return System.Math.Max(tolerance, Core.Tolerance.Distance);
                }

                extents.Sort();
                return System.Math.Max(extents[extents.Count / 2], System.Math.Max(tolerance, Core.Tolerance.Distance));
            }
        }
    }
}
