// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Segment3D> NakedSegment3Ds(this Shell shell, int maxCount = int.MaxValue, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null)
            {
                return null;
            }

            List<Segment3D> result = new List<Segment3D>();

            List<int> faceIndexes = new List<int>();
            List<Segment3D> segment3Ds = new List<Segment3D>();
            for (int i = 0; i < face3Ds.Count; i++)
            {
                Face3D face3D = face3Ds[i];

                ISegmentable3D segmentable3D = face3D?.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D != null)
                {
                    List<Segment3D> segment3Ds_Temp = segmentable3D.GetSegments();
                    if (segment3Ds_Temp != null)
                    {
                        foreach (Segment3D segment3D_Temp in segment3Ds_Temp)
                        {
                            if (segment3D_Temp == null)
                            {
                                continue;
                            }

                            double length_Temp = segment3D_Temp.GetLength();
                            if (length_Temp <= 0 || length_Temp < tolerance)
                            {
                                continue;
                            }

                            faceIndexes.Add(i);
                            segment3Ds.Add(segment3D_Temp);
                        }
                    }
                }

                List<ISegmentable3D> segmentable3Ds_Internal = face3D?.GetInternalEdge3Ds()?.FindAll(x => x is ISegmentable3D)?.ConvertAll(x => (ISegmentable3D)x);
                if (segmentable3Ds_Internal != null)
                {
                    foreach (ISegmentable3D segmentable3D_Internal in segmentable3Ds_Internal)
                    {
                        List<Segment3D> segment3Ds_Temp = segmentable3D_Internal?.GetSegments();
                        if (segment3Ds_Temp == null)
                        {
                            continue;
                        }

                        foreach (Segment3D segment3D_Temp in segment3Ds_Temp)
                        {
                            if (segment3D_Temp == null)
                            {
                                continue;
                            }

                            double length_Temp = segment3D_Temp.GetLength();
                            if (length_Temp <= 0 || length_Temp < tolerance)
                            {
                                continue;
                            }

                            faceIndexes.Add(i);
                            segment3Ds.Add(segment3D_Temp);
                        }
                    }
                }
            }

            int count = segment3Ds.Count;
            if (count == 0)
            {
                return result;
            }

            Point3D[] starts = new Point3D[count];
            Vector3D[] units = new Vector3D[count];
            double[] lengths = new double[count];
            double[] minXs = new double[count];
            double[] maxXs = new double[count];
            double[] minYs = new double[count];
            double[] maxYs = new double[count];
            double[] minZs = new double[count];
            double[] maxZs = new double[count];

            for (int i = 0; i < count; i++)
            {
                Segment3D segment3D = segment3Ds[i];
                Point3D start = segment3D[0];
                Point3D end = segment3D[1];

                starts[i] = start;
                lengths[i] = segment3D.GetLength();
                units[i] = new Vector3D(start, end).GetNormalized();
                minXs[i] = System.Math.Min(start.X, end.X);
                maxXs[i] = System.Math.Max(start.X, end.X);
                minYs[i] = System.Math.Min(start.Y, end.Y);
                maxYs[i] = System.Math.Max(start.Y, end.Y);
                minZs[i] = System.Math.Min(start.Z, end.Z);
                maxZs[i] = System.Math.Max(start.Z, end.Z);
            }

            double[] lengths_Sorted = (double[])lengths.Clone();
            Array.Sort(lengths_Sorted);
            double cellSize = System.Math.Max(lengths_Sorted[count / 2], tolerance > 0 ? tolerance : Core.Tolerance.Distance);

            double epsilon = tolerance > 0 ? tolerance * 1e-6 : 0;
            double tolerance_Expanded = tolerance + epsilon;

            const int maxCellsPerSegment = 4096;

            Dictionary<Tuple<long, long, long>, List<int>> dictionary = new Dictionary<Tuple<long, long, long>, List<int>>();
            List<int> largeIndexes = new List<int>();
            bool[] large = new bool[count];

            for (int i = 0; i < count; i++)
            {
                long kx1 = (long)System.Math.Floor((minXs[i] - tolerance_Expanded) / cellSize);
                long kx2 = (long)System.Math.Floor((maxXs[i] + tolerance_Expanded) / cellSize);
                long ky1 = (long)System.Math.Floor((minYs[i] - tolerance_Expanded) / cellSize);
                long ky2 = (long)System.Math.Floor((maxYs[i] + tolerance_Expanded) / cellSize);
                long kz1 = (long)System.Math.Floor((minZs[i] - tolerance_Expanded) / cellSize);
                long kz2 = (long)System.Math.Floor((maxZs[i] + tolerance_Expanded) / cellSize);

                if ((double)(kx2 - kx1 + 1) * (ky2 - ky1 + 1) * (kz2 - kz1 + 1) > maxCellsPerSegment)
                {
                    large[i] = true;
                    largeIndexes.Add(i);
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

            int[] stamps = new int[count];
            List<Tuple<double, double>> intervals = new List<Tuple<double, double>>();
            List<int> candidates = new List<int>();

            for (int i = 0; i < count; i++)
            {
                Point3D start = starts[i];
                Point3D end = segment3Ds[i][1];
                Vector3D unit = units[i];
                double length = lengths[i];
                int faceIndex = faceIndexes[i];

                double minX = minXs[i] - tolerance_Expanded;
                double maxX = maxXs[i] + tolerance_Expanded;
                double minY = minYs[i] - tolerance_Expanded;
                double maxY = maxYs[i] + tolerance_Expanded;
                double minZ = minZs[i] - tolerance_Expanded;
                double maxZ = maxZs[i] + tolerance_Expanded;

                int stamp = i + 1;
                intervals.Clear();
                candidates.Clear();

                double distance_Start = double.MaxValue;
                double distance_End = double.MaxValue;

                double MinDistance(Point3D point3D)
                {
                    double min = double.MaxValue;
                    foreach (int j in candidates)
                    {
                        double distance_Temp = segment3Ds[j].Distance(point3D);
                        if (distance_Temp < min)
                        {
                            min = distance_Temp;
                        }
                    }

                    return min;
                }

                void ProcessCandidate(int j)
                {
                    if (j == i || stamps[j] == stamp || faceIndexes[j] == faceIndex)
                    {
                        return;
                    }

                    stamps[j] = stamp;

                    if (maxXs[j] < minX || minXs[j] > maxX || maxYs[j] < minY || minYs[j] > maxY || maxZs[j] < minZ || minZs[j] > maxZ)
                    {
                        return;
                    }

                    candidates.Add(j);

                    double distance_Temp = segment3Ds[j].Distance(start);
                    if (distance_Temp < distance_Start)
                    {
                        distance_Start = distance_Temp;
                    }

                    distance_Temp = segment3Ds[j].Distance(end);
                    if (distance_Temp < distance_End)
                    {
                        distance_End = distance_Temp;
                    }

                    Vector3D vector3D_1 = new Vector3D(start, starts[j]);
                    double perpendicular_1 = vector3D_1.CrossProduct(unit).Length;
                    if (perpendicular_1 > tolerance_Expanded)
                    {
                        return;
                    }

                    Vector3D vector3D_2 = new Vector3D(start, segment3Ds[j][1]);
                    double perpendicular_2 = vector3D_2.CrossProduct(unit).Length;
                    if (perpendicular_2 > tolerance_Expanded)
                    {
                        return;
                    }

                    double t1 = vector3D_1.DotProduct(unit);
                    double t2 = vector3D_2.DotProduct(unit);

                    double perpendicular_A = t1 <= t2 ? perpendicular_1 : perpendicular_2;
                    double perpendicular_B = t1 <= t2 ? perpendicular_2 : perpendicular_1;

                    double a = System.Math.Min(t1, t2) - System.Math.Sqrt(System.Math.Max(0, tolerance * tolerance - perpendicular_A * perpendicular_A));
                    double b = System.Math.Max(t1, t2) + System.Math.Sqrt(System.Math.Max(0, tolerance * tolerance - perpendicular_B * perpendicular_B));

                    a = System.Math.Max(a, 0);
                    b = System.Math.Min(b, length);

                    if (a < b)
                    {
                        intervals.Add(new Tuple<double, double>(a, b));
                    }
                }

                if (large[i])
                {
                    for (int j = 0; j < count; j++)
                    {
                        ProcessCandidate(j);
                    }
                }
                else
                {
                    long kx1 = (long)System.Math.Floor(minX / cellSize);
                    long kx2 = (long)System.Math.Floor(maxX / cellSize);
                    long ky1 = (long)System.Math.Floor(minY / cellSize);
                    long ky2 = (long)System.Math.Floor(maxY / cellSize);
                    long kz1 = (long)System.Math.Floor(minZ / cellSize);
                    long kz2 = (long)System.Math.Floor(maxZ / cellSize);

                    for (long kx = kx1; kx <= kx2; kx++)
                    {
                        for (long ky = ky1; ky <= ky2; ky++)
                        {
                            for (long kz = kz1; kz <= kz2; kz++)
                            {
                                if (!dictionary.TryGetValue(new Tuple<long, long, long>(kx, ky, kz), out List<int> list))
                                {
                                    continue;
                                }

                                foreach (int j in list)
                                {
                                    ProcessCandidate(j);
                                }
                            }
                        }
                    }

                    foreach (int j in largeIndexes)
                    {
                        ProcessCandidate(j);
                    }
                }

                double reach = 0;
                if (intervals.Count != 0)
                {
                    intervals.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                    foreach (Tuple<double, double> interval in intervals)
                    {
                        if (interval.Item1 > reach)
                        {
                            double gap = interval.Item1 - reach;

                            bool naked;
                            if (reach == 0)
                            {
                                naked = gap >= tolerance || (gap > epsilon && distance_Start > tolerance_Expanded);
                            }
                            else
                            {
                                naked = gap > epsilon && (gap >= tolerance || MinDistance(new Point3D(start.X + unit.X * (reach + gap / 2), start.Y + unit.Y * (reach + gap / 2), start.Z + unit.Z * (reach + gap / 2))) > tolerance + epsilon);
                            }

                            if (naked)
                            {
                                result.Add(NakedSegment3D(segment3Ds[i], start, unit, reach, interval.Item1, length));
                                if (result.Count >= maxCount)
                                {
                                    return result;
                                }
                            }
                        }

                        if (interval.Item2 > reach)
                        {
                            reach = interval.Item2;
                        }

                        if (reach >= length)
                        {
                            break;
                        }
                    }
                }

                if (reach < length && (length - reach >= tolerance || (length - reach > epsilon && distance_End > tolerance_Expanded)))
                {
                    result.Add(NakedSegment3D(segment3Ds[i], start, unit, reach, length, length));
                    if (result.Count >= maxCount)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        private static Segment3D NakedSegment3D(Segment3D segment3D, Point3D start, Vector3D unit, double from, double to, double length)
        {
            if (from <= 0 && to >= length)
            {
                return segment3D;
            }

            return new Segment3D(
                new Point3D(start.X + unit.X * from, start.Y + unit.Y * from, start.Z + unit.Z * from),
                new Point3D(start.X + unit.X * to, start.Y + unit.Y * to, start.Z + unit.Z * to));
        }
    }
}
