using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Shell : SAMGeometry, IBoundable3D
    {
        private List<Tuple<BoundingBox3D, Face3D>> boundaries;
        private BoundingBox3D boundingBox3D;

        public Shell(IEnumerable<Face3D> face3Ds)
        {
            if (face3Ds != null)
            {
                foreach (Face3D face3D in face3Ds)
                    Add(face3D);
            }
        }

        public Shell(Shell shell)
        {
            if (shell != null)
            {
                boundaries = new List<Tuple<BoundingBox3D, Face3D>>();
                foreach (Tuple<BoundingBox3D, Face3D> boundary in shell.boundaries)
                    boundaries.Add(new Tuple<BoundingBox3D, Face3D>(new BoundingBox3D(boundary.Item1), new Face3D(boundary.Item2)));

                boundingBox3D = shell.boundingBox3D;
            }
        }

        public Shell(JObject jObject)
            : base(jObject)
        {
        }

        private bool Add(Face3D face3D)
        {
            if (face3D == null)
                return false;

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if (boundingBox3D == null)
                return false;

            if (boundaries == null)
                boundaries = new List<Tuple<BoundingBox3D, Face3D>>();

            boundaries.Add(new Tuple<BoundingBox3D, Face3D>(boundingBox3D, new Face3D(face3D)));

            if (this.boundingBox3D == null)
                this.boundingBox3D = new BoundingBox3D(boundingBox3D);
            else
                this.boundingBox3D = new BoundingBox3D(new Point3D[] { this.boundingBox3D.Min, this.boundingBox3D.Max, boundingBox3D.Min, boundingBox3D.Max });

            return true;
        }

        public List<Face3D> Face3Ds
        {
            get
            {
                if (boundaries == null)
                    return null;
                else
                    return boundaries.ConvertAll(x => new Face3D(x.Item2));
            }
        }

        public bool IsClosed(double tolerance = Core.Tolerance.Distance)
        {
            return boundaries != null && boundaries.Count != 0;
        }

        public bool Inside(Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null || boundaries.Count == 0)
                return false;

            if (!IsClosed(tolerance))
                return false;

            BoundingBox3D boundingBox3D_Temp = new BoundingBox3D(boundingBox3D, tolerance);

            if (!boundingBox3D_Temp.InRange(point3D))
                return false;

            Vector3D vector3D = new Vector3D(boundingBox3D_Temp.Min, boundingBox3D_Temp.Max);
            double length = vector3D.Length;
            if (length < silverSpacing)
                return false;

            Segment3D segment3D = new Segment3D(point3D, vector3D);

            List<ISAMGeometry3D> geometry3Ds_Intersection = IntersectionGeometry3Ds<ISAMGeometry3D>(segment3D, false, tolerance);
            if (geometry3Ds_Intersection != null && geometry3Ds_Intersection.Count != 0)
                if (geometry3Ds_Intersection.Find(x => x is ISegmentable3D || OnEdge(x as Point3D, tolerance)) == null)
                    return geometry3Ds_Intersection.Count % 2 != 0;

            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                Point3D point3D_InternalPoint = boundary.Item2.InternalPoint3D(tolerance);
                if (point3D_InternalPoint == null)
                    continue;

                Vector3D vector3D_Temp = new Vector3D(point3D, point3D_InternalPoint);
                vector3D_Temp.Normalize();
                vector3D_Temp = vector3D_Temp * length;

                Segment3D segment3D_Temp = new Segment3D(point3D, vector3D_Temp);
                List<ISAMGeometry3D> geometry3Ds_Intersection_Temp = IntersectionGeometry3Ds<ISAMGeometry3D>(segment3D_Temp, false, tolerance);
                if (geometry3Ds_Intersection_Temp == null || geometry3Ds_Intersection_Temp.Count == 0)
                    continue;

                if (geometry3Ds_Intersection_Temp.Find(x => x is ISegmentable3D || OnEdge(x as Point3D, tolerance)) != null)
                    continue;

                return geometry3Ds_Intersection_Temp.Count % 2 != 0;
            }

            return false;
        }

        public List<Point3D> IntersectionPoint3Ds(Segment3D segment3D, bool includeInternalEdges = true, double tolerance = Core.Tolerance.Distance)
        {
            return IntersectionGeometry3Ds<Point3D>(segment3D, includeInternalEdges, tolerance);
        }

        public List<T> IntersectionGeometry3Ds<T>(Segment3D segment3D, bool includeInternalEdges = true, double tolerance = Core.Tolerance.Distance) where T: ISAMGeometry3D
        {
            if (segment3D == null || boundaries == null)
                return null;

            BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();
            if (!boundingBox3D.InRange(boundingBox3D_Segment3D))
                return null;

            HashSet<T> sAMGeometry3Ds = new HashSet<T>();
            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                if (!boundary.Item1.InRange(boundingBox3D_Segment3D))
                    continue;
                
                PlanarIntersectionResult planarIntersectionResult = null;

                if (includeInternalEdges)
                    planarIntersectionResult = Create.PlanarIntersectionResult(boundary.Item2, segment3D, tolerance);
                else
                    planarIntersectionResult = Create.PlanarIntersectionResult(new Face3D(boundary.Item2.GetExternalEdge3D()), segment3D, tolerance);

                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<T> sAMGeometry3Ds_Temp = planarIntersectionResult.GetGeometry3Ds<T>();
                if (sAMGeometry3Ds_Temp != null && sAMGeometry3Ds_Temp.Count > 0)
                    sAMGeometry3Ds_Temp.ForEach(x => sAMGeometry3Ds.Add(x));
            }

            return sAMGeometry3Ds.ToList();
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null)
                return false;

            if (!boundingBox3D.InRange(point3D, tolerance))
                return false;

            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                if (!boundary.Item1.InRange(point3D, tolerance))
                    continue;

                if (boundary.Item2.On(point3D, tolerance))
                    return true;
            }

            return false;
        }

        public bool OnEdge(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null)
                return false;

            if (!boundingBox3D.InRange(point3D, tolerance))
                return false;

            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                if (!boundary.Item1.InRange(point3D, tolerance))
                    continue;

                if (boundary.Item2.OnEdge(point3D, tolerance))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks in Point3D is in range (on shell excluding internal edges of Faces3D). It does not check if Point3D is inside shell
        /// </summary>
        /// <param name="point3D">SAM Geometry Point3D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>True if Point3D is in range</returns>
        public bool InRange(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null)
                return false;

            if (!boundingBox3D.InRange(point3D, tolerance))
                return false;

            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                if (!boundary.Item1.InRange(point3D, tolerance))
                    continue;

                if (boundary.Item2.InRange(point3D, tolerance))
                    return true;
            }

            return false;
        }

        public List<Face3D> ClosestFace3Ds(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            List<int> indexes = ClosestFace3DsIndexes(point3D, tolerance);
            if (indexes == null || indexes.Count == 0)
                return null;

            return indexes.ConvertAll(x => new Face3D(boundaries[x].Item2));
        }

        public List<int> ClosestFace3DsIndexes(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            if (boundaries == null || boundaries.Count == 0)
                return null;

            if (boundaries.Count == 1)
                return new List<int>() { 0 };

            List<int> result = new List<int>();

            List<double> distances = boundaries.ConvertAll(x => x.Item2.Distance(point3D));
            double min = distances.Min();
            for (int i = 0; i < distances.Count; i++)
            {
                if (System.Math.Abs(distances[i] - min) <= tolerance)
                    result.Add(i);
            }

            return result;
        }

        public List<Point3D> ClosestPoint3Ds(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = ClosestFace3Ds(point3D, tolerance);
            if (face3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>();
            foreach (Face3D face3D in face3Ds)
            {
                Point3D point3D_Closest = face3D.Closest(point3D);
                if (point3D_Closest != null)
                    result.Add(point3D_Closest);
            }

            return result;
        }

        public Vector3D Normal(Point3D point3D, bool external = false, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<int> indexes = ClosestFace3DsIndexes(point3D, tolerance);
            if (indexes == null || indexes.Count == 0)
                return null;

            return Normal(indexes[0], external, silverSpacing, tolerance);


            //List<Face3D> face3Ds = ClosestFace3Ds(point3D, tolerance);
            //if (face3Ds == null || face3Ds.Count == 0)
            //    return null;

            //Face3D face3D = face3Ds[0];

            //Vector3D vector3D = face3D?.GetPlane()?.Normal;
            //if (!external || !IsClosed(tolerance))
            //    return vector3D;

            //vector3D *= silverSpacing;

            //Point3D point3D_Move = point3D.GetMoved(vector3D) as Point3D;
            //if (Inside(point3D_Move, silverSpacing, tolerance))
            //    vector3D.Negate();

            //return vector3D.Unit;
        }

        public Vector3D Normal(int index, bool external = false, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (boundaries == null || boundaries.Count == 0)
                return null;

            if (index < 0 || index >= boundaries.Count)
                return null;

            Face3D face3D = boundaries[index].Item2;
            if (face3D == null)
                return null;

            Vector3D vector3D = face3D?.GetPlane()?.Normal;
            if (!external || !IsClosed(tolerance))
                return vector3D;

            Point3D point3D = face3D.InternalPoint3D(tolerance);
            if (point3D == null)
                return null;

            vector3D *= silverSpacing;

            Point3D point3D_Move = point3D.GetMoved(vector3D) as Point3D;
            if (Inside(point3D_Move, silverSpacing, tolerance))
                vector3D.Negate();

            return vector3D.Unit;
        }

        public Face3D this[int index]
        {
            get
            {
                Face3D result = boundaries?[index].Item2;
                if (result == null)
                    return result;

                return new Face3D(result);
            }
        }
               
        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            if (boundingBox3D == null)
                return boundingBox3D;
            else
                return new BoundingBox3D(boundingBox3D, offset);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Face3Ds"))
            {
                List<Face3D> face3Ds = Geometry.Create.ISAMGeometries<Face3D>(jObject.Value<JArray>("Face3Ds"));
                if (face3Ds != null)
                    face3Ds.ForEach(x => Add(x));
            }
            return true;
        }

        public List<ICurve3D> GetCurve3Ds(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            List<ICurve3D> result = new List<ICurve3D>();
            foreach (Tuple<BoundingBox3D, Face3D> tuple in boundaries)
            {
                if (!tuple.Item1.InRange(point3D, tolerance))
                    continue;

                if (!tuple.Item2.GetPlane().On(point3D, tolerance))
                    continue;

                ISegmentable3D segmentable3D = tuple.Item1.GetExternalEdge() as ISegmentable3D;
                if (segmentable3D == null)
                    throw new NotImplementedException();

                foreach(Segment3D segment3D in segmentable3D.GetSegments())
                {
                    if (segment3D.On(point3D, tolerance))
                        result.Add(segment3D);
                }
            }
            return result;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (boundaries != null)
                jObject.Add("Face3Ds", Geometry.Create.JArray(boundaries.ConvertAll(x => x.Item2)));

            return jObject;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            if (vector3D == null)
                return null;

            if (boundaries == null)
                return new Shell(new List<Face3D>());

            List<Face3D> face3Ds = boundaries.ConvertAll(x => x.Item2.GetMoved(vector3D) as Face3D);
            return new Shell(face3Ds);
        }

        public override ISAMGeometry Clone()
        {
            return new Shell(this);
        }

        public Point3D InternalPoint3D(double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (boundaries == null || boundaries.Count == 0 || boundingBox3D == null)
                return null;

            if (!IsClosed(tolerance))
                return null;

            Point3D result = boundingBox3D.GetCenter();
            if (Inside(result, silverSpacing, tolerance))
                return result;

            List<Tuple<BoundingBox3D, Face3D>> boundaries_Temp = new List<Tuple<BoundingBox3D, Face3D>>(boundaries);
            boundaries_Temp.Sort((x, y) => x.Item1.Max.Z.CompareTo(y.Item1.Max.Z));

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries_Temp)
            {
                Point3D point3D_Internal = boundary.Item2.InternalPoint3D();
                if (result == null)
                    continue;

                Vector3D normal = null;

                if (System.Math.Abs(boundary.Item1.Max.Z - boundary.Item1.Min.Z) <= tolerance)
                {
                    normal = Vector3D.WorldZ * (boundingBox3D.Height / 2);

                    if (System.Math.Abs(boundary.Item1.Max.Z - boundingBox3D.Max.Z) <= tolerance)
                        normal = normal.GetNegated();

                    result = (Point3D)point3D_Internal.GetMoved(normal);
                    if (Inside(result, silverSpacing, tolerance))
                        return result;

                    normal = normal.Unit * silverSpacing;

                    result = (Point3D)point3D_Internal.GetMoved(normal);
                    if (Inside(result, silverSpacing, tolerance))
                        return result;
                }

                point3Ds.Add(result);

                normal = Normal(result, true, silverSpacing, tolerance)?.GetNegated();
                if (normal == null)
                    continue;

                result = (Point3D)point3D_Internal.GetMoved(normal);
                if (Inside(result, silverSpacing, tolerance))
                    return result;
            }

            for(int i=0; i < point3Ds.Count - 1; i++)
            {
                for (int j = i + 1; j < point3Ds.Count; j++)
                {
                    List<Point3D> point3D_Intersections = IntersectionPoint3Ds(new Segment3D(point3Ds[i], point3Ds[j]), false, tolerance);
                    if (point3D_Intersections == null || point3D_Intersections.Count < 2)
                        continue;

                    for (int k = 0; k < point3D_Intersections.Count - 1; k++)
                    {
                        result = point3D_Intersections[k].Mid(point3D_Intersections[k + 1]);
                        if (Inside(result, silverSpacing, tolerance))
                            return result;
                    }
                }
            }

            return null;
        }

        public bool Simplify(double tolerance = Core.Tolerance.Distance)
        {
            if (boundaries == null || boundaries.Count == 0)
                return false;

            for (int i = 0; i < boundaries.Count; i++)
            {
                Face3D face3D = Query.SimplifyByNTS_TopologyPreservingSimplifier(boundaries[i].Item2, tolerance);
                if (face3D == null)
                    continue;

                boundaries[i] = new Tuple<BoundingBox3D, Face3D>(face3D.GetBoundingBox(), face3D);
            }

            boundingBox3D = new BoundingBox3D(boundaries.ConvertAll(x => x.Item1));
            return true;
        }

        public void OrientNormals(bool flipX = false, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            Dictionary<Face3D, Vector3D> dictionary = Query.NormalDictionary(this, true, silverSpacing, tolerance);
            if (dictionary == null)
                return;

            boundaries = new List<Tuple<BoundingBox3D, Face3D>>();
            foreach(KeyValuePair<Face3D, Vector3D> keyValuePair in dictionary)
            {
                Face3D face3D = keyValuePair.Key;

                Vector3D normal_External = keyValuePair.Value;
                if (normal_External != null)
                {
                    Vector3D normal_Face3D = face3D.GetPlane()?.Normal;
                    if(normal_Face3D != null && !normal_External.SameHalf(normal_Face3D))
                        face3D.FlipNormal(flipX);
                }

                Add(face3D);
            }
        }

        public bool SplitFace3Ds(Face3D face3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (boundaries == null || boundaries.Count == 0)
                return false;

            BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
            if (boundingBox3D == null)
                return false;

            if (!this.boundingBox3D.InRange(boundingBox3D, tolerance_Distance))
                return false;

            Plane plane = face3D.GetPlane();

            Dictionary<int, List<Face3D>> dictionary = new Dictionary<int, List<Face3D>>();
            for(int i= boundaries.Count - 1; i >= 0; i--)
            {
                if (!boundaries[i].Item1.InRange(boundingBox3D))
                    continue;

                Face3D face3D_Boundary = boundaries[i].Item2;

                Plane plane_Boundary = face3D_Boundary.GetPlane();

                if (plane_Boundary.Normal.SmallestAngle(plane.Normal.GetNegated()) > tolerance_Angle && plane_Boundary.Normal.SmallestAngle(plane.Normal) > tolerance_Angle)
                    continue;

                if (face3D.Distance(face3D_Boundary) >= tolerance_Distance)
                    continue;

                Face2D face2D_Boundary = plane_Boundary.Convert(face3D_Boundary);
                Face2D face2D = plane_Boundary.Convert(plane_Boundary.Project(face3D));

                List<Face2D> face2Ds = face2D_Boundary.Intersection(face2D, tolerance_Distance);
                face2Ds?.RemoveAll(x => x == null || x.GetArea() <= tolerance_Distance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                if (face2Ds.Count == 1 && System.Math.Abs(face2Ds[0].GetArea() - face2D_Boundary.GetArea()) <= tolerance_Distance)
                {
                    continue;
                }

                List<Face2D> face2Ds_Difference = face2D_Boundary.Difference(face2Ds);
                face2Ds_Difference?.RemoveAll(x => x == null || x.GetArea() <= tolerance_Distance);
                if (face2Ds_Difference != null && face2Ds_Difference.Count != 0)
                {
                    face2Ds.AddRange(face2Ds_Difference);
                }

                dictionary[i] = face2Ds.ConvertAll(x => plane_Boundary.Convert(x));
            }

            if(dictionary == null || dictionary.Count == 0)
            {
                return false;
            }

            foreach(KeyValuePair<int, List<Face3D>> keyValuePair in dictionary)
            {
                boundaries.RemoveAt(keyValuePair.Key);
                foreach (Face3D face3D_New in keyValuePair.Value)
                {
                    boundaries.Add(new Tuple<BoundingBox3D, Face3D>(face3D_New.GetBoundingBox(), face3D_New));
                }
            }

            this.boundingBox3D = new BoundingBox3D(boundaries.ConvertAll(x => x.Item1));
            return true;
        }

        public bool SplitFace3Ds(Shell shell, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shell == null)
                return false;

            if (!boundingBox3D.InRange(shell.boundingBox3D, tolerance_Distance))
                return false;

            List<Tuple<BoundingBox3D, Face3D>> boundaries_Temp = shell.boundaries;
            if (boundaries_Temp == null || boundaries_Temp.Count == 0)
                return false;

            if (boundaries == null || boundaries.Count == 0)
                return false;

            bool result = false;
            foreach (Tuple<BoundingBox3D, Face3D> boundary_Temp in boundaries_Temp)
            {
                if(!boundingBox3D.InRange(boundary_Temp.Item1, tolerance_Distance))
                {
                    continue;
                }

                if (SplitFace3Ds(boundary_Temp.Item2, tolerance_Angle, tolerance_Distance))
                {
                    result = true;
                }
            }

            return result;
        }
    }
}