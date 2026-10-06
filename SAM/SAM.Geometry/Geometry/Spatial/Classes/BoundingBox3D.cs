// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Geometry.Spatial
{
    /// <summary>
    /// Represents a 3D axis-aligned bounding box (AABB).
    /// </summary>
    public class BoundingBox3D : SAMGeometry, IClosed3D, ISegmentable3D
    {
        private Point3D min;
        private Point3D max;

        /// <summary>
        /// Gets the minimum X coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MinX => min?.X ?? double.NaN;

        /// <summary>
        /// Gets the minimum Y coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MinY => min?.Y ?? double.NaN;

        /// <summary>
        /// Gets the minimum Z coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MinZ => min?.Z ?? double.NaN;

        /// <summary>
        /// Gets the maximum X coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MaxX => max?.X ?? double.NaN;

        /// <summary>
        /// Gets the maximum Y coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MaxY => max?.Y ?? double.NaN;

        /// <summary>
        /// Gets the maximum Z coordinate of the bounding box, or double.NaN if uninitialized.
        /// </summary>
        public double MaxZ => max?.Z ?? double.NaN;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class enclosing the given points.
        /// </summary>
        /// <param name="point3Ds">The collection of 3D points.</param>
        public BoundingBox3D(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null)
            {
                return;
            }

            bool hasPoints = false;
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            double aZ_Min = double.MaxValue;
            double aZ_Max = double.MinValue;

            foreach (Point3D point3D in point3Ds)
            {
                if (point3D == null)
                {
                    continue;
                }

                hasPoints = true;
                if (point3D.X > aX_Max)
                {
                    aX_Max = point3D.X;
                }
                if (point3D.X < aX_Min)
                {
                    aX_Min = point3D.X;
                }
                if (point3D.Y > aY_Max)
                {
                    aY_Max = point3D.Y;
                }
                if (point3D.Y < aY_Min)
                {
                    aY_Min = point3D.Y;
                }
                if (point3D.Z > aZ_Max)
                {
                    aZ_Max = point3D.Z;
                }
                if (point3D.Z < aZ_Min)
                {
                    aZ_Min = point3D.Z;
                }
            }

            if (hasPoints)
            {
                min = new Point3D(aX_Min, aY_Min, aZ_Min);
                max = new Point3D(aX_Max, aY_Max, aZ_Max);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class enclosing two points.
        /// </summary>
        /// <param name="point3D_1">The first point.</param>
        /// <param name="point3D_2">The second point.</param>
        public BoundingBox3D(Point3D point3D_1, Point3D point3D_2)
        {
            if (point3D_1 == null && point3D_2 == null)
            {
                return;
            }

            if (point3D_1 == null)
            {
                min = new Point3D(point3D_2);
                max = new Point3D(point3D_2);
                return;
            }

            if (point3D_2 == null)
            {
                min = new Point3D(point3D_1);
                max = new Point3D(point3D_1);
                return;
            }

            min = new Point3D(System.Math.Min(point3D_1.X, point3D_2.X), System.Math.Min(point3D_1.Y, point3D_2.Y), System.Math.Min(point3D_1.Z, point3D_2.Z));
            max = new Point3D(System.Math.Max(point3D_1.X, point3D_2.X), System.Math.Max(point3D_1.Y, point3D_2.Y), System.Math.Max(point3D_1.Z, point3D_2.Z));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class enclosing two points with an offset.
        /// </summary>
        /// <param name="point3D_1">The first point.</param>
        /// <param name="point3D_2">The second point.</param>
        /// <param name="offset">The offset distance expand boundaries.</param>
        public BoundingBox3D(Point3D point3D_1, Point3D point3D_2, double offset)
        {
            if (point3D_1 == null && point3D_2 == null)
            {
                return;
            }

            if (point3D_1 == null)
            {
                min = new Point3D(point3D_2.X - offset, point3D_2.Y - offset, point3D_2.Z - offset);
                max = new Point3D(point3D_2.X + offset, point3D_2.Y + offset, point3D_2.Z + offset);
                return;
            }

            if (point3D_2 == null)
            {
                min = new Point3D(point3D_1.X - offset, point3D_1.Y - offset, point3D_1.Z - offset);
                max = new Point3D(point3D_1.X + offset, point3D_1.Y + offset, point3D_1.Z + offset);
                return;
            }

            min = new Point3D(System.Math.Min(point3D_1.X, point3D_2.X) - offset, System.Math.Min(point3D_1.Y, point3D_2.Y) - offset, System.Math.Min(point3D_1.Z, point3D_2.Z) - offset);
            max = new Point3D(System.Math.Max(point3D_1.X, point3D_2.X) + offset, System.Math.Max(point3D_1.Y, point3D_2.Y) + offset, System.Math.Max(point3D_1.Z, point3D_2.Z) + offset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class centered at a point expanded by an offset.
        /// </summary>
        /// <param name="point3D">The center point.</param>
        /// <param name="offset">The half-extent offset.</param>
        public BoundingBox3D(Point3D point3D, double offset)
        {
            if (point3D == null)
            {
                return;
            }

            min = new Point3D(point3D.X - offset, point3D.Y - offset, point3D.Z - offset);
            max = new Point3D(point3D.X + offset, point3D.Y + offset, point3D.Z + offset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class enclosing points expanded by an offset.
        /// </summary>
        /// <param name="point3Ds">The collection of points.</param>
        /// <param name="offset">The offset distance to expand boundaries.</param>
        public BoundingBox3D(IEnumerable<Point3D> point3Ds, double offset)
        {
            if (point3Ds == null)
            {
                return;
            }

            bool hasPoints = false;
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            double aZ_Min = double.MaxValue;
            double aZ_Max = double.MinValue;

            foreach (Point3D point3D in point3Ds)
            {
                if (point3D == null)
                {
                    continue;
                }

                hasPoints = true;
                if (point3D.X > aX_Max)
                {
                    aX_Max = point3D.X;
                }
                if (point3D.X < aX_Min)
                {
                    aX_Min = point3D.X;
                }
                if (point3D.Y > aY_Max)
                {
                    aY_Max = point3D.Y;
                }
                if (point3D.Y < aY_Min)
                {
                    aY_Min = point3D.Y;
                }
                if (point3D.Z > aZ_Max)
                {
                    aZ_Max = point3D.Z;
                }
                if (point3D.Z < aZ_Min)
                {
                    aZ_Min = point3D.Z;
                }
            }

            if (hasPoints)
            {
                min = new Point3D(aX_Min - offset, aY_Min - offset, aZ_Min - offset);
                max = new Point3D(aX_Max + offset, aY_Max + offset, aZ_Max + offset);
            }
        }

        /// <summary>
        /// Initializes a new copy instance of the <see cref="BoundingBox3D"/> class.
        /// </summary>
        /// <param name="boundingBox3D">The source bounding box.</param>
        public BoundingBox3D(BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D?.min != null && boundingBox3D?.max != null)
            {
                min = new Point3D(boundingBox3D.min);
                max = new Point3D(boundingBox3D.max);
            }
        }

        /// <summary>
        /// Initializes a new copy instance of the <see cref="BoundingBox3D"/> class expanded by an offset.
        /// </summary>
        /// <param name="boundingBox3D">The source bounding box.</param>
        /// <param name="offset">The offset distance.</param>
        public BoundingBox3D(BoundingBox3D boundingBox3D, double offset)
        {
            if (boundingBox3D?.min != null && boundingBox3D?.max != null)
            {
                min = new Point3D(boundingBox3D.min.X - offset, boundingBox3D.min.Y - offset, boundingBox3D.min.Z - offset);
                max = new Point3D(boundingBox3D.max.X + offset, boundingBox3D.max.Y + offset, boundingBox3D.max.Z + offset);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class enclosing multiple bounding boxes.
        /// </summary>
        /// <param name="boundingBox3Ds">The collection of bounding boxes.</param>
        public BoundingBox3D(IEnumerable<BoundingBox3D> boundingBox3Ds)
        {
            if (boundingBox3Ds == null)
            {
                return;
            }

            bool hasBoxes = false;
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            double aZ_Min = double.MaxValue;
            double aZ_Max = double.MinValue;

            foreach (BoundingBox3D boundingBox3D in boundingBox3Ds)
            {
                if (boundingBox3D?.min == null || boundingBox3D?.max == null)
                {
                    continue;
                }

                hasBoxes = true;
                if (boundingBox3D.min.X < aX_Min)
                {
                    aX_Min = boundingBox3D.min.X;
                }
                if (boundingBox3D.max.X > aX_Max)
                {
                    aX_Max = boundingBox3D.max.X;
                }
                if (boundingBox3D.min.Y < aY_Min)
                {
                    aY_Min = boundingBox3D.min.Y;
                }
                if (boundingBox3D.max.Y > aY_Max)
                {
                    aY_Max = boundingBox3D.max.Y;
                }
                if (boundingBox3D.min.Z < aZ_Min)
                {
                    aZ_Min = boundingBox3D.min.Z;
                }
                if (boundingBox3D.max.Z > aZ_Max)
                {
                    aZ_Max = boundingBox3D.max.Z;
                }
            }

            if (hasBoxes)
            {
                min = new Point3D(aX_Min, aY_Min, aZ_Min);
                max = new Point3D(aX_Max, aY_Max, aZ_Max);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox3D"/> class from JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public BoundingBox3D(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Determines whether this bounding box intersects another bounding box.
        /// </summary>
        /// <param name="boundingBox3D">The target bounding box.</param>
        /// <returns>True if they intersect; otherwise, false.</returns>
        public bool Intersect(BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D?.min == null || boundingBox3D?.max == null || min == null || max == null)
            {
                return false;
            }

            return min.X <= boundingBox3D.max.X && max.X >= boundingBox3D.min.X &&
                   min.Y <= boundingBox3D.max.Y && max.Y >= boundingBox3D.min.Y &&
                   min.Z <= boundingBox3D.max.Z && max.Z >= boundingBox3D.min.Z;
        }

        /// <summary>
        /// Determines whether this bounding box intersects a 3D line segment using a 0-allocation Slab algorithm.
        /// </summary>
        /// <param name="segment3D">The 3D segment.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if the segment intersects or lies within the bounding box; otherwise, false.</returns>
        public bool Intersect(Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null || min == null || max == null)
            {
                return false;
            }

            Point3D p0 = segment3D[0];
            Point3D p1 = segment3D[1];
            if (p0 == null || p1 == null)
            {
                return false;
            }

            double tMin = 0.0;
            double tMax = 1.0;

            // Threshold below which a direction component is treated as parallel to the axis (segment does not cross that slab).
            const double parallelEpsilon = 1e-15;

            double boxMinX = min.X - tolerance;
            double boxMaxX = max.X + tolerance;
            double dx = p1.X - p0.X;
            if (System.Math.Abs(dx) < parallelEpsilon)
            {
                if (p0.X < boxMinX || p0.X > boxMaxX)
                {
                    return false;
                }
            }
            else
            {
                double invD = 1.0 / dx;
                double t1 = (boxMinX - p0.X) * invD;
                double t2 = (boxMaxX - p0.X) * invD;
                double tNear = System.Math.Min(t1, t2);
                double tFar = System.Math.Max(t1, t2);
                tMin = System.Math.Max(tMin, tNear);
                tMax = System.Math.Min(tMax, tFar);
                if (tMin > tMax)
                {
                    return false;
                }
            }

            double boxMinY = min.Y - tolerance;
            double boxMaxY = max.Y + tolerance;
            double dy = p1.Y - p0.Y;
            if (System.Math.Abs(dy) < parallelEpsilon)
            {
                if (p0.Y < boxMinY || p0.Y > boxMaxY)
                {
                    return false;
                }
            }
            else
            {
                double invD = 1.0 / dy;
                double t1 = (boxMinY - p0.Y) * invD;
                double t2 = (boxMaxY - p0.Y) * invD;
                double tNear = System.Math.Min(t1, t2);
                double tFar = System.Math.Max(t1, t2);
                tMin = System.Math.Max(tMin, tNear);
                tMax = System.Math.Min(tMax, tFar);
                if (tMin > tMax)
                {
                    return false;
                }
            }

            double boxMinZ = min.Z - tolerance;
            double boxMaxZ = max.Z + tolerance;
            double dz = p1.Z - p0.Z;
            if (System.Math.Abs(dz) < parallelEpsilon)
            {
                if (p0.Z < boxMinZ || p0.Z > boxMaxZ)
                {
                    return false;
                }
            }
            else
            {
                double invD = 1.0 / dz;
                double t1 = (boxMinZ - p0.Z) * invD;
                double t2 = (boxMaxZ - p0.Z) * invD;
                double tNear = System.Math.Min(t1, t2);
                double tFar = System.Math.Max(t1, t2);
                tMin = System.Math.Max(tMin, tNear);
                tMax = System.Math.Min(tMax, tFar);
                if (tMin > tMax)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the minimum corner point of the bounding box.
        /// </summary>
        public Point3D Min
        {
            get
            {
                return min == null ? null : new Point3D(min);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (max == null)
                {
                    max = new Point3D(value);
                    min = new Point3D(value);
                }
                else
                {
                    max = Query.Max(max, value);
                    min = Query.Min(max, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum corner point of the bounding box.
        /// </summary>
        public Point3D Max
        {
            get
            {
                return max == null ? null : new Point3D(max);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (min == null)
                {
                    max = new Point3D(value);
                    min = new Point3D(value);
                }
                else
                {
                    max = Query.Max(min, value);
                    min = Query.Min(min, value);
                }
            }
        }

        /// <summary>
        /// Gets the width (X dimension extent) of the bounding box.
        /// </summary>
        public double Width => (max != null && min != null) ? max.X - min.X : double.NaN;

        /// <summary>
        /// Gets the height (Z dimension extent) of the bounding box.
        /// </summary>
        public double Height => (max != null && min != null) ? max.Z - min.Z : double.NaN;

        /// <summary>
        /// Gets the depth (Y dimension extent) of the bounding box.
        /// </summary>
        public double Depth => (max != null && min != null) ? max.Y - min.Y : double.NaN;

        /// <summary>
        /// Gets the six boundary planes of the box.
        /// </summary>
        /// <returns>A list of planes bounding the box.</returns>
        public List<Plane> GetPlanes()
        {
            if (min == null || max == null)
            {
                return new List<Plane>();
            }

            List<Plane> planes = new List<Plane>();
            planes.Add(new Plane(min, Vector3D.WorldX));
            planes.Add(new Plane(min, Vector3D.WorldY));
            planes.Add(new Plane(min, Vector3D.WorldZ));
            planes.Add(new Plane(max, Vector3D.WorldX));
            planes.Add(new Plane(max, Vector3D.WorldY));
            planes.Add(new Plane(max, Vector3D.WorldZ));
            return planes;
        }

        /// <summary>
        /// Determines whether another bounding box is strictly inside this bounding box.
        /// </summary>
        /// <param name="boundingBox3D">The target bounding box.</param>
        /// <returns>True if strictly inside; otherwise, false.</returns>
        public bool Inside(BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D?.min == null || boundingBox3D?.max == null)
            {
                return false;
            }

            return Inside(boundingBox3D.max) && Inside(boundingBox3D.min);
        }

        /// <summary>
        /// Determines whether a point is strictly inside this bounding box.
        /// </summary>
        /// <param name="point3D">The 3D point.</param>
        /// <returns>True if strictly inside; otherwise, false.</returns>
        public bool Inside(Point3D point3D)
        {
            if (point3D == null || min == null || max == null)
            {
                return false;
            }

            return point3D.X > min.X && point3D.X < max.X &&
                   point3D.Y > min.Y && point3D.Y < max.Y &&
                   point3D.Z > min.Z && point3D.Z < max.Z;
        }

        /// <summary>
        /// Determines whether a point is inside this bounding box with tolerance option for edge inclusion.
        /// </summary>
        /// <param name="point3D">The 3D point.</param>
        /// <param name="acceptOnEdge">Whether points on the box surface/edge are accepted as inside.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if inside; otherwise, false.</returns>
        public bool Inside(Point3D point3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || min == null || max == null)
            {
                return false;
            }

            if (acceptOnEdge)
            {
                return point3D.X >= min.X - tolerance && point3D.X <= max.X + tolerance &&
                       point3D.Y >= min.Y - tolerance && point3D.Y <= max.Y + tolerance &&
                       point3D.Z >= min.Z - tolerance && point3D.Z <= max.Z + tolerance;
            }

            return point3D.X > min.X + tolerance && point3D.X < max.X - tolerance &&
                   point3D.Y > min.Y + tolerance && point3D.Y < max.Y - tolerance &&
                   point3D.Z > min.Z + tolerance && point3D.Z < max.Z - tolerance;
        }

        /// <summary>
        /// Determines whether a line segment is inside this bounding box.
        /// </summary>
        /// <param name="segment3D">The line segment.</param>
        /// <param name="acceptOnEdge">Whether points on the edge are accepted.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if both endpoints are inside; otherwise, false.</returns>
        public bool Inside(Segment3D segment3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
            {
                return false;
            }

            if (!Inside(segment3D[0], acceptOnEdge, tolerance))
            {
                return false;
            }

            if (!Inside(segment3D[1], acceptOnEdge, tolerance))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if another bounding box is in range of this bounding box within tolerance.
        /// </summary>
        /// <param name="boundingBox3D">The other bounding box.</param>
        /// <param name="tolerance">Tolerance distance.</param>
        /// <returns>True if in range; otherwise, false.</returns>
        public bool InRange(BoundingBox3D boundingBox3D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.InRange(this, boundingBox3D, tolerance);
        }

        /// <summary>
        /// Clones this bounding box.
        /// </summary>
        /// <returns>A new <see cref="BoundingBox3D"/> instance.</returns>
        public override ISAMGeometry Clone()
        {
            return new BoundingBox3D(this);
        }

        /// <summary>
        /// Calculates the total surface area of all 6 faces of the 3D bounding box.
        /// </summary>
        /// <returns>Total 3D surface area.</returns>
        public double GetArea()
        {
            if (min == null || max == null)
            {
                return double.NaN;
            }

            double w = Width;
            double h = Height;
            double d = Depth;
            return 2.0 * ((w * d) + (w * h) + (d * h));
        }

        /// <summary>
        /// Calculates the volume of the bounding box.
        /// </summary>
        /// <returns>Volume.</returns>
        public double GetVolume()
        {
            if (min == null || max == null)
            {
                return double.NaN;
            }

            return Width * Height * Depth;
        }

        /// <summary>
        /// Gets the 12 edge segments of the bounding box.
        /// </summary>
        /// <returns>List of 12 3D segments.</returns>
        public List<Segment3D> GetSegments()
        {
            if (min == null || max == null)
            {
                return new List<Segment3D>();
            }

            double x = Width;
            double z = Height;
            double y = Depth;

            List<Segment3D> result = new List<Segment3D>();
            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X + x, min.Y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z), new Point3D(min.X + x, min.Y + y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z), new Point3D(min.X, min.Y + y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z), new Point3D(min)));

            result.Add(new Segment3D(new Point3D(min.X, min.Y, min.Z + z), new Point3D(min.X + x, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z + z), new Point3D(min.X + x, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z + z), new Point3D(min.X, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z + z), new Point3D(min.X, min.Y, min.Z + z)));

            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z), new Point3D(min.X + x, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z), new Point3D(min.X + x, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z), new Point3D(min.X, min.Y + y, min.Z + z)));
            return result;
        }

        /// <summary>
        /// Gets the 8 corner points of the bounding box.
        /// </summary>
        /// <returns>List of 8 3D points.</returns>
        public List<Point3D> GetPoints()
        {
            if (min == null || max == null)
            {
                return new List<Point3D>();
            }

            double x = Width;
            double z = Height;
            double y = Depth;

            List<Point3D> point3Ds = new List<Point3D>();
            point3Ds.Add(new Point3D(min));
            point3Ds.Add(new Point3D(min.X + x, min.Y, min.Z));
            point3Ds.Add(new Point3D(min.X + x, min.Y + y, min.Z));
            point3Ds.Add(new Point3D(min.X, min.Y + y, min.Z));

            point3Ds.Add(new Point3D(min.X, min.Y, max.Z));
            point3Ds.Add(new Point3D(min.X + x, min.Y, max.Z));
            point3Ds.Add(new Point3D(max));
            point3Ds.Add(new Point3D(min.X, min.Y + y, max.Z));

            return point3Ds;
        }

        /// <summary>
        /// Gets a bounding box expanded by an offset.
        /// </summary>
        /// <param name="offset">Offset distance.</param>
        /// <returns>A new <see cref="BoundingBox3D"/> instance.</returns>
        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(this, offset);
        }

        /// <summary>
        /// Gets external boundary geometry.
        /// </summary>
        /// <returns>External closed 3D boundary.</returns>
        public IClosed3D GetExternalEdge()
        {
            return new BoundingBox3D(this);
        }

        /// <summary>
        /// Gets curves representing box edges.
        /// </summary>
        /// <returns>List of curves.</returns>
        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x);
        }

        /// <summary>
        /// Gets the center point of the bounding box.
        /// </summary>
        /// <returns>The centroid point.</returns>
        public Point3D GetCentroid()
        {
            if (min == null || max == null)
            {
                return null;
            }

            return Query.Mid(min, max);
        }

        /// <summary>
        /// Moves the bounding box by a vector.
        /// </summary>
        /// <param name="vector3D">Translation vector.</param>
        /// <returns>Moved bounding box.</returns>
        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            if (min == null || max == null || vector3D == null)
            {
                return null;
            }

            return new BoundingBox3D((Point3D)min.GetMoved(vector3D), (Point3D)max.GetMoved(vector3D));
        }

        /// <summary>
        /// Deserializes bounding box properties from JSON object.
        /// </summary>
        /// <param name="jsonObject">Source JSON object.</param>
        /// <returns>True if deserialized successfully; otherwise, false.</returns>
        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return false;
            }

            if (jsonObject["Max"] is JsonObject jsonObject_Max)
            {
                max = new Point3D((JsonObject)jsonObject_Max.DeepClone());
            }

            if (jsonObject["Min"] is JsonObject jsonObject_Min)
            {
                min = new Point3D((JsonObject)jsonObject_Min.DeepClone());
            }

            return true;
        }

        /// <summary>
        /// Serializes bounding box properties to JSON object.
        /// </summary>
        /// <returns>Serialized JSON object.</returns>
        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
            {
                return null;
            }

            if (max?.ToJsonObject() is JsonObject maxJson)
            {
                jsonObject["Max"] = maxJson.DeepClone();
            }

            if (min?.ToJsonObject() is JsonObject minJson)
            {
                jsonObject["Min"] = minJson.DeepClone();
            }

            return jsonObject;
        }

        /// <summary>
        /// Checks if a point lies on any edge of the bounding box.
        /// </summary>
        /// <param name="point3D">Target point.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if on edge; otherwise, false.</returns>
        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(this, point3D, tolerance);
        }

        /// <summary>
        /// Expands the bounding box to include another bounding box.
        /// </summary>
        /// <param name="boundingBox3D">Bounding box to include.</param>
        /// <returns>True if expanded or modified; otherwise, false.</returns>
        public bool Include(BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D?.min == null || boundingBox3D?.max == null)
            {
                return false;
            }

            if (min == null || max == null)
            {
                min = new Point3D(boundingBox3D.min);
                max = new Point3D(boundingBox3D.max);
                return true;
            }

            min = new Point3D(System.Math.Min(min.X, boundingBox3D.min.X), System.Math.Min(min.Y, boundingBox3D.min.Y), System.Math.Min(min.Z, boundingBox3D.min.Z));
            max = new Point3D(System.Math.Max(max.X, boundingBox3D.max.X), System.Math.Max(max.Y, boundingBox3D.max.Y), System.Math.Max(max.Z, boundingBox3D.max.Z));
            return true;
        }

        /// <summary>
        /// Expands the bounding box to include a point.
        /// </summary>
        /// <param name="point3D">Point to include.</param>
        /// <returns>True if expanded or modified; otherwise, false.</returns>
        public bool Include(Point3D point3D)
        {
            if (point3D == null)
            {
                return false;
            }

            if (min == null || max == null)
            {
                min = new Point3D(point3D);
                max = new Point3D(point3D);
                return true;
            }

            min = new Point3D(System.Math.Min(min.X, point3D.X), System.Math.Min(min.Y, point3D.Y), System.Math.Min(min.Z, point3D.Z));
            max = new Point3D(System.Math.Max(max.X, point3D.X), System.Math.Max(max.Y, point3D.Y), System.Math.Max(max.Z, point3D.Z));
            return true;
        }

        /// <summary>
        /// Expands the bounding box to include a collection of points in a single pass.
        /// </summary>
        /// <param name="point3Ds">Points to include.</param>
        /// <returns>True if expanded or modified; otherwise, false.</returns>
        public bool Include(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null)
            {
                return false;
            }

            bool modified = false;
            double minX = min?.X ?? double.MaxValue;
            double minY = min?.Y ?? double.MaxValue;
            double minZ = min?.Z ?? double.MaxValue;
            double maxX = max?.X ?? double.MinValue;
            double maxY = max?.Y ?? double.MinValue;
            double maxZ = max?.Z ?? double.MinValue;

            foreach (Point3D pt in point3Ds)
            {
                if (pt == null)
                {
                    continue;
                }

                modified = true;
                if (pt.X < minX)
                {
                    minX = pt.X;
                }
                if (pt.Y < minY)
                {
                    minY = pt.Y;
                }
                if (pt.Z < minZ)
                {
                    minZ = pt.Z;
                }
                if (pt.X > maxX)
                {
                    maxX = pt.X;
                }
                if (pt.Y > maxY)
                {
                    maxY = pt.Y;
                }
                if (pt.Z > maxZ)
                {
                    maxZ = pt.Z;
                }
            }

            if (modified)
            {
                min = new Point3D(minX, minY, minZ);
                max = new Point3D(maxX, maxY, maxZ);
            }

            return modified;
        }

        /// <summary>
        /// Transforms the bounding box using a 3D transformation.
        /// </summary>
        /// <param name="transform3D">Transformation.</param>
        /// <returns>Transformed bounding box.</returns>
        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        /// <summary>
        /// Gets the sum of all 12 edge lengths of the bounding box.
        /// </summary>
        /// <returns>Total edge length.</returns>
        public double GetLength()
        {
            if (min == null || max == null)
            {
                return double.NaN;
            }

            return (4.0 * Height) + (4.0 * Width) + (4.0 * Depth);
        }

        /// <summary>
        /// Compares equality with another object.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True if equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            BoundingBox3D boundingBox3D = obj as BoundingBox3D;
            if (boundingBox3D == null)
            {
                return false;
            }

            return boundingBox3D.max == max && boundingBox3D.min == min;
        }

        /// <summary>
        /// Computes the hash code for this bounding box.
        /// </summary>
        /// <returns>Hash code integer.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (min?.GetHashCode() ?? 0);
                hash = (hash * 23) + (max?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2)
        {
            if (ReferenceEquals(boundingBox3D_1, null) && ReferenceEquals(boundingBox3D_2, null))
            {
                return true;
            }

            if (ReferenceEquals(boundingBox3D_1, null))
            {
                return false;
            }

            if (ReferenceEquals(boundingBox3D_2, null))
            {
                return false;
            }

            return boundingBox3D_1.min == boundingBox3D_2.min && boundingBox3D_1.max == boundingBox3D_2.max;
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2)
        {
            if (ReferenceEquals(boundingBox3D_1, null) && ReferenceEquals(boundingBox3D_2, null))
            {
                return false;
            }

            if (ReferenceEquals(boundingBox3D_1, null))
            {
                return true;
            }

            if (ReferenceEquals(boundingBox3D_2, null))
            {
                return true;
            }

            return boundingBox3D_1.min != boundingBox3D_2.min || boundingBox3D_1.max != boundingBox3D_2.max;
        }
    }
}
