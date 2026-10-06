// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    public class Polygon2DTests
    {
        private static Polygon2D UnitSquare()
        {
            return new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1),
            });
        }

        private static Polygon2D UnitTriangle()
        {
            return new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(0.5, 1),
            });
        }

        private static Polygon2D OffsetSquare(double offset)
        {
            return new Polygon2D(new List<Point2D>()
            {
                new Point2D(offset, offset),
                new Point2D(offset + 1, offset),
                new Point2D(offset + 1, offset + 1),
                new Point2D(offset, offset + 1),
            });
        }

        // ── GetLength ───────────────────────────────────────────────

        [Fact]
        public void GetLength_UnitSquare_Returns4()
        {
            Polygon2D square = UnitSquare();
            double length = square.GetLength();
            Assert.True(System.Math.Abs(length - 4.0) < Core.Tolerance.Distance,
                $"Expected 4.0, got {length}");
        }

        [Fact]
        public void GetLength_RightTriangle_ReturnsPerimeter()
        {
            Polygon2D triangle = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(3, 0),
                new Point2D(0, 4),
            });

            double perimeter = 3.0 + 4.0 + 5.0; // 3-4-5 triangle
            double length = triangle.GetLength();
            Assert.True(System.Math.Abs(length - perimeter) < Core.Tolerance.Distance,
                $"Expected {perimeter}, got {length}");
        }

        [Fact]
        public void GetLength_Degenerate_LessThan2Points_ReturnsZero()
        {
            Polygon2D empty = new Polygon2D(new List<Point2D>());
            Polygon2D single = new Polygon2D(new List<Point2D>() { new Point2D(0, 0) });

            Assert.Equal(0, empty.GetLength());
            Assert.Equal(0, single.GetLength());
        }

        [Fact]
        public void GetLength_LargePositiveCoordinates()
        {
            const double s = 1e6;
            Polygon2D poly = new Polygon2D(new List<Point2D>()
            {
                new Point2D(s, s),
                new Point2D(s + 100, s),
                new Point2D(s + 100, s + 100),
                new Point2D(s, s + 100),
            });

            Assert.True(System.Math.Abs(poly.GetLength() - 400.0) < Core.Tolerance.Distance);
        }

        // ── On ──────────────────────────────────────────────────────

        [Fact]
        public void On_PointOnVertex_ReturnsTrue()
        {
            Polygon2D square = UnitSquare();
            Assert.True(square.On(new Point2D(0, 0)));
            Assert.True(square.On(new Point2D(1, 0)));
            Assert.True(square.On(new Point2D(1, 1)));
            Assert.True(square.On(new Point2D(0, 1)));
        }

        [Fact]
        public void On_PointOnEdgeMidpoint_ReturnsTrue()
        {
            Polygon2D square = UnitSquare();
            Assert.True(square.On(new Point2D(0.5, 0)));   // bottom edge
            Assert.True(square.On(new Point2D(1, 0.5)));   // right edge
            Assert.True(square.On(new Point2D(0.5, 1)));   // top edge
            Assert.True(square.On(new Point2D(0, 0.5)));   // left edge
        }

        [Fact]
        public void On_PointInsidePolygon_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.On(new Point2D(0.5, 0.5)));
        }

        [Fact]
        public void On_PointOutsidePolygon_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.On(new Point2D(2, 2)));
        }

        [Fact]
        public void On_PointJustWithinTolerance_ReturnsTrue()
        {
            Polygon2D square = UnitSquare();
            double tol = Core.Tolerance.Distance;
            double offset = tol * 0.5;

            Assert.True(square.On(new Point2D(0.5, offset), tol));
            Assert.True(square.On(new Point2D(0.5, 1 - offset), tol));
        }

        [Fact]
        public void On_PointJustOutsideTolerance_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            double tol = Core.Tolerance.Distance;
            double offset = tol * 1.5;

            Assert.False(square.On(new Point2D(0.5, -offset), tol));
            Assert.False(square.On(new Point2D(0.5, 1 + offset), tol));
        }

        [Fact]
        public void On_NullPoint_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.On(null));
        }

        [Fact]
        public void On_DegeneratePolygon_ReturnsFalse()
        {
            Polygon2D poly = new Polygon2D(new List<Point2D>() { new Point2D(0, 0) });
            Assert.False(poly.On(new Point2D(0, 0)));
        }

        [Fact]
        public void On_NegativeTolerance_ReturnsFalse()
        {
            Assert.False(UnitSquare().On(new Point2D(0.5, 0), -1));
        }

        [Fact]
        public void On_ZeroTolerance_UsesStrictLessThan()
        {
            Polygon2D square = UnitSquare();

            Assert.False(square.On(new Point2D(0.5, 0), 0),
                "tolerance=0 uses strict <, even exact distance=0 returns false");

            Assert.False(square.On(new Point2D(0.5, 1e-9), 0));
        }

        [Fact]
        public void On_NaNTolerance_ReturnsFalse()
        {
            Assert.False(UnitSquare().On(new Point2D(0.5, 0), double.NaN));
        }

        [Fact]
        public void On_PositiveInfinityTolerance_ReturnsTrue()
        {
            Assert.True(UnitSquare().On(new Point2D(10, 10), double.PositiveInfinity));
        }

        [Fact]
        public void On_DuplicateConsecutivePoints_HandledCorrectly()
        {
            Polygon2D poly = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(0, 0), // duplicate
                new Point2D(1, 0),
                new Point2D(1, 1),
            });

            Assert.True(poly.On(new Point2D(0, 0)));
            Assert.True(poly.On(new Point2D(0.5, 0)));
        }

        [Fact]
        public void On_ZeroLengthEdge_HandledCorrectly()
        {
            Polygon2D poly = new Polygon2D(new List<Point2D>()
            {
                new Point2D(2, 2),
                new Point2D(2, 2),
                new Point2D(5, 2),
                new Point2D(5, 5),
            });

            Assert.True(poly.On(new Point2D(2, 2)));
            Assert.True(poly.On(new Point2D(3.5, 2)));
        }

        [Fact]
        public void On_NaNCoordinateInPolygon_ReturnsFalse()
        {
            Polygon2D poly = new Polygon2D(new List<Point2D>()
            {
                new Point2D(double.NaN, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
            });

            Assert.False(poly.On(new Point2D(0.5, 0)));
        }

        [Fact]
        public void On_ExactBoundaryTolerance_Consistent()
        {
            Polygon2D square = UnitSquare();
            double tol = Core.Tolerance.Distance;

            Assert.True(square.On(new Point2D(0.5, tol * 0.999999), tol));
            Assert.False(square.On(new Point2D(0.5, tol * 1.000001), tol));
        }

        // ── Inside (Point2D) ────────────────────────────────────────

        [Fact]
        public void Inside_Point_InsidePolygon_ReturnsTrue()
        {
            Polygon2D square = UnitSquare();
            Assert.True(square.Inside(new Point2D(0.5, 0.5)));
        }

        [Fact]
        public void Inside_Point_OutsidePolygon_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.Inside(new Point2D(2, 2)));
        }

        [Fact]
        public void Inside_Point_OnEdge_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.Inside(new Point2D(0.5, 0)));
            Assert.False(square.Inside(new Point2D(0, 0)));
            Assert.False(square.Inside(new Point2D(0.5, 1)));
        }

        [Fact]
        public void Inside_NullPoint_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.Inside((Point2D)null));
        }

        // ── Inside (IClosed2D) ──────────────────────────────────────

        [Fact]
        public void Inside_IClosed2D_SmallerSquareInsideLarger_ReturnsTrue()
        {
            Polygon2D outer = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
            });

            Polygon2D inner = new Polygon2D(new List<Point2D>()
            {
                new Point2D(3, 3),
                new Point2D(7, 3),
                new Point2D(7, 7),
                new Point2D(3, 7),
            });

            Assert.True(outer.Inside(inner));
        }

        [Fact]
        public void Inside_IClosed2D_PolygonOverlapping_ReturnsFalse()
        {
            Polygon2D square1 = UnitSquare();
            Polygon2D square2 = OffsetSquare(0.5);

            Assert.False(square1.Inside(square2));
        }

        [Fact]
        public void Inside_IClosed2D_NullInput_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.Inside((IClosed2D)null));
        }

        [Fact]
        public void Inside_IClosed2D_PolygonContainsAnother_ReturnsTrue()
        {
            Polygon2D outer = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
            });

            Polygon2D inner = UnitSquare(); // (0,0)-(1,1) is inside outer

            Assert.True(outer.Inside(inner));
        }

        // ── GetHashCode ─────────────────────────────────────────────

        [Fact]
        public void GetHashCode_SamePolygon_SameHash()
        {
            Polygon2D a = UnitSquare();
            Polygon2D b = new Polygon2D(a.GetPoints());

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentPolygons_DifferentHash()
        {
            Polygon2D a = UnitSquare();
            Polygon2D b = UnitTriangle();

            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }

        // ── SetOrientation ──────────────────────────────────────────

        [Fact]
        public void SetOrientation_Undefined_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.SetOrientation(Orientation.Undefined));
        }

        [Fact]
        public void SetOrientation_Collinear_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.SetOrientation(Orientation.Collinear));
        }

        [Fact]
        public void SetOrientation_UnitsSquare_CanFlipAndRestore()
        {
            Polygon2D square = UnitSquare();
            Orientation original = square.GetOrientation();

            bool result = square.SetOrientation(
                original == Orientation.Clockwise ? Orientation.CounterClockwise : Orientation.Clockwise);

            Assert.True(result);
            Assert.NotEqual(original, square.GetOrientation());

            result = square.SetOrientation(original);
            Assert.True(result);
            Assert.Equal(original, square.GetOrientation());
        }

        // ── Reorder ─────────────────────────────────────────────────

        [Fact]
        public void Reorder_DifferentStartIndex_SamePointsInRotatedOrder()
        {
            Polygon2D square = UnitSquare();
            List<Point2D> original = square.GetPoints();

            bool result = square.Reorder(2);
            Assert.True(result);

            List<Point2D> reordered = square.GetPoints();
            Assert.True(original[2].X == reordered[0].X && original[2].Y == reordered[0].Y,
                $"Expected first point to be original index 2, got ({reordered[0].X}, {reordered[0].Y})");
        }

        [Fact]
        public void Reorder_InvalidIndex_ReturnsFalse()
        {
            Polygon2D square = UnitSquare();
            Assert.False(square.Reorder(-1));
            Assert.False(square.Reorder(100));
        }

        // ── Reverse ─────────────────────────────────────────────────

        [Fact]
        public void Reverse_KeepFirstPoint_FirstPointUnchanged()
        {
            Polygon2D square = UnitSquare();
            Point2D first = new Point2D(square.GetPoints()[0]);
            Point2D second = new Point2D(square.GetPoints()[1]);
            Point2D last = new Point2D(square.GetPoints()[square.Count - 1]);

            square.Reverse(true);

            List<Point2D> reversed = square.GetPoints();
            Assert.True(first.AlmostEquals(reversed[0], Core.Tolerance.Distance));
            Assert.True(last.AlmostEquals(reversed[1], Core.Tolerance.Distance));
            Assert.True(second.AlmostEquals(reversed[reversed.Count - 1], Core.Tolerance.Distance));
        }

        [Fact]
        public void Reverse_NoParameter_ReversesWithKeepFirstPoint()
        {
            Polygon2D square = UnitSquare();
            Point2D first = new Point2D(square.GetPoints()[0]);

            square.Reverse();

            List<Point2D> reversed = square.GetPoints();
            Assert.True(first.AlmostEquals(reversed[0], Core.Tolerance.Distance));
        }

        // ── GetEnumerator ───────────────────────────────────────────

        [Fact]
        public void GetEnumerator_Iteration_YieldsAllPoints()
        {
            Polygon2D square = UnitSquare();
            List<Point2D> points = square.GetPoints();
            int expected = points.Count;
            int count = 0;

            foreach (Point2D p in square)
            {
                Assert.True(points[count].AlmostEquals(p, Core.Tolerance.Distance));
                count++;
            }

            Assert.Equal(expected, count);
        }

        // ── Closest ─────────────────────────────────────────────────

        [Fact]
        public void Closest_PointOutside_ReturnsClosestEdgePoint()
        {
            Polygon2D square = UnitSquare();
            Point2D result = square.Closest(new Point2D(2, 0.5), includeEdges: true);

            Assert.True(System.Math.Abs(result.X - 1.0) < Core.Tolerance.Distance);
            Assert.True(System.Math.Abs(result.Y - 0.5) < Core.Tolerance.Distance);
        }

        // ── GetArea ─────────────────────────────────────────────────

        [Fact]
        public void GetArea_UnitSquare_Returns1()
        {
            Polygon2D square = UnitSquare();
            Assert.True(System.Math.Abs(square.GetArea() - 1.0) < Core.Tolerance.Distance);
        }

        [Fact]
        public void GetArea_UnitTriangle_ReturnsHalf()
        {
            Polygon2D triangle = UnitTriangle();
            Assert.True(System.Math.Abs(triangle.GetArea() - 0.5) < Core.Tolerance.Distance);
        }

        // ── Inside + On consistency ─────────────────────────────────

        [Fact]
        public void Inside_OnEdge_AreConsistent()
        {
            Polygon2D square = UnitSquare();
            Point2D onEdge = new Point2D(0.5, 0);

            Assert.True(square.On(onEdge));
            Assert.False(square.Inside(onEdge));
        }

        [Fact]
        public void On_Inside_Outside_AreMutuallyExclusive()
        {
            Polygon2D square = UnitSquare();
            Point2D inside = new Point2D(0.5, 0.5);
            Point2D on = new Point2D(0.5, 0);
            Point2D outside = new Point2D(2, 2);

            Assert.True(square.Inside(inside));
            Assert.False(square.On(inside));

            Assert.False(square.Inside(on));
            Assert.True(square.On(on));

            Assert.False(square.Inside(outside));
            Assert.False(square.On(outside));
        }

        // ── Move ────────────────────────────────────────────────────

        [Fact]
        public void Move_ByVector_TranslatesAllPoints()
        {
            Polygon2D square = UnitSquare();
            Vector2D vector = new Vector2D(5, 3);

            Assert.True(square.Move(vector));

            List<Point2D> points = square.GetPoints();
            Assert.True(System.Math.Abs(points[0].X - 5) < Core.Tolerance.Distance);
            Assert.True(System.Math.Abs(points[0].Y - 3) < Core.Tolerance.Distance);
            Assert.True(System.Math.Abs(points[2].X - 6) < Core.Tolerance.Distance);
            Assert.True(System.Math.Abs(points[2].Y - 4) < Core.Tolerance.Distance);
        }

        // ── GetCentroid ─────────────────────────────────────────────

        [Fact]
        public void GetCentroid_UnitSquare_AtCenter()
        {
            Polygon2D square = UnitSquare();
            Point2D centroid = square.GetCentroid();

            Assert.True(System.Math.Abs(centroid.X - 0.5) < Core.Tolerance.Distance);
            Assert.True(System.Math.Abs(centroid.Y - 0.5) < Core.Tolerance.Distance);
        }

        // ── GetInternalPoint2D ──────────────────────────────────────

        [Fact]
        public void GetInternalPoint2D_UnitSquare_InsidePolygon()
        {
            Polygon2D square = UnitSquare();
            Point2D internalPoint = square.GetInternalPoint2D();

            Assert.True(square.Inside(internalPoint));
            Assert.False(square.On(internalPoint));
        }

        // ── Polygon2Ds (extension) ──────────────────────────────────

        [Fact]
        public void Polygon2Ds_NullInput_ReturnsNull()
        {
            List<Polygon2D> result = ((IEnumerable<ISegmentable2D>)null).Polygon2Ds();
            Assert.Null(result);
        }

        [Fact]
        public void Polygon2Ds_EmptyInput_ReturnsEmptyList()
        {
            List<Polygon2D> result = new List<ISegmentable2D>().Polygon2Ds();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Polygon2Ds_SingleSquare_ReturnsOnePolygon()
        {
            Polyline2D polyline = new Polyline2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1),
                new Point2D(0, 0),
            });

            List<Polygon2D> result = new List<ISegmentable2D>() { polyline }.Polygon2Ds();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(System.Math.Abs(result[0].GetArea() - 1.0) < Core.Tolerance.Distance);
        }

        [Fact]
        public void Polygon2Ds_DuplicateExternalPolygons_DedupedToSingle()
        {
            Polyline2D polyline = new Polyline2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1),
                new Point2D(0, 0),
            });

            List<Polygon2D> result = new List<ISegmentable2D>() { polyline, polyline }.Polygon2Ds();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(System.Math.Abs(result[0].GetArea() - 1.0) < Core.Tolerance.Distance);
        }

        [Fact]
        public void Polygon2Ds_TwoDistinctSquares_ReturnsTwoPolygons()
        {
            Polyline2D polyline1 = new Polyline2D(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1),
                new Point2D(0, 0),
            });

            Polyline2D polyline2 = new Polyline2D(new List<Point2D>()
            {
                new Point2D(10, 10),
                new Point2D(11, 10),
                new Point2D(11, 11),
                new Point2D(10, 11),
                new Point2D(10, 10),
            });

            List<Polygon2D> result = new List<ISegmentable2D>() { polyline1, polyline2 }.Polygon2Ds();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Polygon2Ds_SplitOverload_RespectsSplitFlag()
        {
            List<ISegmentable2D> input = new List<ISegmentable2D>()
            {
                new Polyline2D(new List<Point2D>()
                {
                    new Point2D(0, 0),
                    new Point2D(1, 0),
                    new Point2D(1, 1),
                    new Point2D(0, 1),
                    new Point2D(0, 0),
                }),
            };

            List<Polygon2D> noSplit = input.Polygon2Ds(split: false);
            List<Polygon2D> withSplit = input.Polygon2Ds(split: true);

            Assert.NotNull(noSplit);
            Assert.NotNull(withSplit);
        }

        // ── ToJsonObject / FromJsonObject ───────────────────────────

        [Fact]
        public void Json_RoundTrip_PreservesGeometry()
        {
            Polygon2D square = UnitSquare();
            System.Text.Json.Nodes.JsonObject json = square.ToJsonObject();

            Polygon2D restored = new Polygon2D(json);

            Assert.Equal(square.Count, restored.Count);
            Assert.True(System.Math.Abs(square.GetArea() - restored.GetArea()) < Core.Tolerance.Distance);

            List<Point2D> originalPoints = square.GetPoints();
            List<Point2D> restoredPoints = restored.GetPoints();
            for (int i = 0; i < originalPoints.Count; i++)
                Assert.True(originalPoints[i].AlmostEquals(restoredPoints[i], Core.Tolerance.Distance));
        }

        // ── GetSegments ─────────────────────────────────────────────

        [Fact]
        public void GetSegments_UnitSquare_Returns4ClosedSegments()
        {
            Polygon2D square = UnitSquare();
            List<Segment2D> segments = square.GetSegments();

            Assert.Equal(4, segments.Count);
        }

        // ── ResetPoint ──────────────────────────────────────────────

        [Fact]
        public void InsertClosest_PointOnEdge_AddsVertex()
        {
            Polygon2D square = UnitSquare();
            int originalCount = square.Count;

            Point2D inserted = square.InsertClosest(new Point2D(0.5, 0));

            Assert.NotNull(inserted);
            Assert.True(square.On(inserted, Core.Tolerance.Distance));
            Assert.True(square.Count > originalCount);
        }

        // ── GetMoved ────────────────────────────────────────────────

        [Fact]
        public void GetMoved_ReturnsNewPolygon_OriginalUnchanged()
        {
            Polygon2D square = UnitSquare();
            double originalArea = square.GetArea();

            Polygon2D moved = square.GetMoved(new Vector2D(10, 20));

            Assert.NotNull(moved);
            Assert.True(System.Math.Abs(originalArea - square.GetArea()) < Core.Tolerance.Distance,
                "Original should be unchanged");
            Assert.True(System.Math.Abs(moved.GetArea() - originalArea) < Core.Tolerance.Distance);
        }

        // ── Clone ───────────────────────────────────────────────────

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            Polygon2D original = UnitSquare();
            Polygon2D clone = (Polygon2D)original.Clone();

            Assert.NotNull(clone);
            Assert.Equal(original.Count, clone.Count);
            Assert.True(System.Math.Abs(original.GetArea() - clone.GetArea()) < Core.Tolerance.Distance);

            clone.Move(new Vector2D(10, 10));
            Assert.False(System.Math.Abs(original.GetCentroid().X - clone.GetCentroid().X) < Core.Tolerance.Distance,
                "Clone move should not affect original");
        }

        // ── Similar & Dedup at Tolerance Boundary ───────────────────

        [Fact]
        public void Similar_ShapesWithinToleranceBoundary_ReturnsTrue()
        {
            double tol = Core.Tolerance.Distance;
            Polygon2D poly1 = UnitSquare();
            // Offset vertices by 0.5 * tol (strictly within tolerance)
            Polygon2D poly2 = new Polygon2D(new List<Point2D>
            {
                new Point2D(0 + 0.5 * tol, 0),
                new Point2D(1 + 0.5 * tol, 0),
                new Point2D(1 + 0.5 * tol, 1),
                new Point2D(0 + 0.5 * tol, 1)
            });

            Assert.True(SAM.Geometry.Planar.Query.Similar((IClosed2D)poly1, (IClosed2D)poly2, tol));
        }

        [Fact]
        public void Similar_ShapesBeyondToleranceBoundary_ReturnsFalse()
        {
            double tol = Core.Tolerance.Distance;
            Polygon2D poly1 = UnitSquare();
            // Offset vertices by 2 * tol
            Polygon2D poly2 = new Polygon2D(new List<Point2D>
            {
                new Point2D(0 + 2 * tol, 0),
                new Point2D(1 + 2 * tol, 0),
                new Point2D(1 + 2 * tol, 1),
                new Point2D(0 + 2 * tol, 1)
            });

            Assert.False(SAM.Geometry.Planar.Query.Similar((IClosed2D)poly1, (IClosed2D)poly2, tol));
        }

        [Fact]
        public void Polygon2Ds_PolygonsAtToleranceBoundary_DeduplicatesCorrectly()
        {
            double tol = Core.Tolerance.MicroDistance;
            Polyline2D polyline1 = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1),
                new Point2D(0, 0)
            });

            Polyline2D polyline2 = new Polyline2D(new List<Point2D>
            {
                new Point2D(0 + tol * 0.5, 0),
                new Point2D(1 + tol * 0.5, 0),
                new Point2D(1 + tol * 0.5, 1),
                new Point2D(0 + tol * 0.5, 1),
                new Point2D(0 + tol * 0.5, 0)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { polyline1, polyline2 }.Polygon2Ds(tolerance: tol);

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
