using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry
{
    public static partial class Query
    {
        /// <summary>
        /// Sanp Panels to given Grid2Ds
        /// </summary>
        /// <param name="face3DObjects">List of the Face3D Objects to be snapped</param>
        /// <param name="elevation">Section elevation</param>
        /// <param name="bucketSizes">Distance to select local panels (squize into one)</param>
        /// <param name="maxGaps">Max distance for panels Extension</param>
        /// <param name="weights">Weight for panels</param>
        /// <param name="tolerance_Angle">Angle tolerance</param>
        /// <param name="tolerance_Distance">Distance tolerance</param>

        private static void Sample(this List<IFace3DObject> face3DObjects,
            double elevation,
            Func<IFace3DObject, double> bucketSizes,
            Func<IFace3DObject, double> maxGaps,
            Func<IFace3DObject, double> weights,
            double tolerance_Angle = Core.Tolerance.Angle,
            double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3DObjects == null)
            {
                return;
            }

            //Create section plane (Plane on given elevation)
            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            //
            // Panels to 2D geometry
            //

            //Get geometry of panels have been created by section. Dictionary alows to receive panel object for given section geometry.
            //ISegmentable2D is 2D geometry which can be described by connected segments. Examples of ISegmentable2D: Polyline, Polygon (closed polyline), rectangle, triangle, BoundingBox etc.
            Dictionary<IFace3DObject, List<ISegmentable2D>> dictionary = face3DObjects.SectionDictionary<IFace3DObject, ISegmentable2D>(plane, tolerance_Distance);

            //Temporary list to collect all the Segment2Ds for Panels section
            List<Segment2D> segment2Ds_Temp = new List<Segment2D>();

            //Temporary list to collect all the ISegmentable2Ds for Panels section
            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();

            foreach (KeyValuePair<IFace3DObject, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                //Panel is analytical representation of Wall, Floor, Roof etc.
                IFace3DObject face3DObject = keyValuePair.Key;

                //Get weight for the given panel to determine sanp behaviour
                double weight = weights(face3DObject);

                //Get bucketSize for the given panel
                double bucketSize = bucketSizes(face3DObject);

                //Get max Gap for the given panel
                double maxGap = maxGaps(face3DObject);

                //Panel geometry is always planar! Use Face3D object to retreive panel Geometry. Face3D contains External Edge and Internal Edges
                Face3D face3D = face3DObject.Face3D; //Face3D is similar to Polygon2D however it may contains internal edges ("holes")

                //BoundingBox3D of Face3D
                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();

                //You can get intersection geometry using PlanarIntersectionResult
                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, Core.Tolerance.Angle, tolerance_Distance);

                //Line below is example how to get intersection 2D geometry from PlanarIntersectionResult
                List<ISegmentable2D> segmentable2Ds_Intersection = planarIntersectionResult.GetGeometry2Ds<ISegmentable2D>();

                //This is example how to get intersection 3D geometry from PlanarIntersectionResult
                List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();

                //Iterating through section geometry for panel
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    //In the most of the cases section geometry will be single segment.
                    List<Segment2D> segment2Ds = segmentable2D.GetSegments();
                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        segment2Ds_Temp.Add(segment2D);
                    }

                    segmentable2Ds.Add(segmentable2D);
                }

                //If we assume that section of one panel creates single segment2D then
                //we can extract all the points from segmentable2Ds and then gets extremes
                List<Point2D> point2Ds = keyValuePair.Value.UniquePoint2Ds(tolerance_Distance);
                point2Ds.ExtremePoints(out Point2D point2D_1, out Point2D point2D_2);
                if (point2D_1.Distance(point2D_2) >= tolerance_Distance)
                {
                    Segment2D segment2D_Panel = new Segment2D(point2D_1, point2D_2);
                }

            }

            //Split given list of segment2Ds to add intersection points
            segment2Ds_Temp = segment2Ds_Temp.Split(tolerance_Distance);

            //Find all closed loops created by given segment2Ds
            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds_Temp, tolerance_Distance);

            //Gets External Polygon2Ds
            List<Polygon2D> polygon2Ds_External = Geometry.Planar.Query.ExternalPolygon2Ds(polygon2Ds);

            //Converting 2D geometry to 3D geometry on given plane
            List<Polygon3D> polygon3Ds = polygon2Ds.ConvertAll(x => plane.Convert(x));

            //
            //Example of 2D operations on segments:
            //

            Segment2D segment2D_1 = segment2Ds_Temp.First();
            Segment2D segment2D_2 = segment2Ds_Temp.Last();

            //segment direction (unit vector)
            Vector2D vector2D = segment2D_1.Direction;

            //Intersection of two segments. Second parameter determines if segments are bounded. If bouned sets to false then intersection point may not lay on given segments
            Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, true, tolerance_Distance);

            //Closest point on segment to given point2D
            Point2D point2D_Closest = segment2D_1.Closest(segment2D_2[0]);

            //BoundingBox for given segment2Ds:
            BoundingBox2D boundingBox2D_1 = segment2D_1.GetBoundingBox();
            BoundingBox2D boundingBox2D_2 = segment2D_2.GetBoundingBox();

            //Check if boundingBox2D_2 is in range of boundingBox2D_1. This can be used as initial check for intersection when perforamnce is the priority
            if (boundingBox2D_1.InRange(boundingBox2D_2, tolerance_Distance))
            {
                //Another way to get intersection information for two segments. point2D_Closest_1 is closest point on segment2D_1 to intersection Point2D, point2D_Closest_2 is closest point on segment2D_2 
                Point2D point2D_Intersection_Temp = segment2D_1.Intersection(segment2D_2, out Point2D point2D_Closest_1, out Point2D point2D_Closest2, tolerance_Distance);

                //Check if Point2D of segment2D_2 is on segment2D_1
                bool isOn = segment2D_1.On(segment2D_2[0], tolerance_Distance);
            }

            //All intersections between segment and list of segmentables
            List<Point2D> point2Ds_Intersections = segment2D_1.Intersections(segmentable2Ds, tolerance_Distance);

            //
            //Tracing sample
            //

            //To receive ray trace data use TraceData Query. Inputs: start point, direction, list of geometry will be check for ray hit. Outputs: tuple with hit point, segment being hit, and hit direction
            List<Tuple<Point2D, Segment2D, Vector2D>> traceData = Geometry.Planar.Query.TraceData(segment2D_1[0], segment2D_1.Direction, segmentable2Ds);

            //Fast way to receive first hit
            Vector2D vector2D_RayTrace = Geometry.Planar.Query.TraceFirst(segment2D_1[0], segment2D_1.Direction, segmentable2Ds);

            //
            //Bool Operations sample. Similar methods for Face2D and Polygon2D
            //

            Polygon2D polygon2D_1 = polygon2Ds.First();
            Polygon2D polygon2D_2 = polygon2Ds.Last();

            List<Polygon2D> polygon2Ds_Intersection = polygon2D_1.Intersection(polygon2D_2, tolerance_Distance);
            List<Polygon2D> polygon2Ds_Union = polygon2D_1.Union(polygon2D_2, tolerance_Distance);
            List<Polygon2D> polygon2Ds_Difference = polygon2D_1.Difference(polygon2D_2, tolerance_Distance);
        }
    }
}
