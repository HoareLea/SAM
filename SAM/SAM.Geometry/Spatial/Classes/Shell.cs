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
            return true;
        }

        public bool Inside(Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null)
                return false;

            if (!IsClosed(tolerance))
                return false;

            BoundingBox3D boundingBox3D_Temp = new BoundingBox3D(boundingBox3D, tolerance);

            if (!boundingBox3D_Temp.Inside(point3D))
                return false;

            Vector3D vector3D = new Vector3D(boundingBox3D_Temp.Min, boundingBox3D_Temp.Max);
            if (vector3D.Length < silverSpacing)
                return false;

            Segment3D segment3D = new Segment3D(point3D, vector3D);

            List<Point3D> point3Ds = IntersectionPoint3Ds(segment3D, false, tolerance);
            if (point3Ds == null || point3Ds.Count == 0)
                return false;

            return point3Ds.Count % 2 != 0;
        }

        public List<Point3D> IntersectionPoint3Ds(Segment3D segment3D, bool includeInternalEdges = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null || boundaries == null)
                return null;
            
            HashSet<Point3D> point3Ds = new HashSet<Point3D>();
            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                PlanarIntersectionResult planarIntersectionResult = null;

                if(includeInternalEdges)
                    planarIntersectionResult = PlanarIntersectionResult.Create(boundary.Item2, segment3D, tolerance);
                else
                    planarIntersectionResult = PlanarIntersectionResult.Create(new Face3D(boundary.Item2.GetExternalEdge()), segment3D, tolerance);

                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Point3D> point3Ds_Temp = planarIntersectionResult.GetGeometry3Ds<Point3D>();
                if (point3Ds_Temp != null && point3Ds_Temp.Count > 0)
                    point3Ds_Temp.ForEach(x => point3Ds.Add(x));
            }

            return point3Ds.ToList();
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

            if (boundaries == null || boundaries.Count == 0)
                return null;

            if (boundaries.Count == 1)
                return new List<Face3D>() { boundaries[0].Item2 };

            List<Face3D> result = new List<Face3D>();

            List<double> distances = boundaries.ConvertAll(x => x.Item2.Distance(point3D));
            double min = distances.Min();
            for (int i = 0; i < distances.Count; i++)
            {
                if (System.Math.Abs(distances[i] - min) <= tolerance)
                    result.Add(new Face3D(boundaries[i].Item2));
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
            List<Face3D> face3Ds = ClosestFace3Ds(point3D, tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            Face3D face3D = face3Ds[0];

            Vector3D vector3D = face3D?.GetPlane()?.Normal;
            if (!external || !IsClosed(tolerance))
                return vector3D;

            vector3D *= silverSpacing;

            Point3D point3D_Move = point3D.GetMoved(vector3D) as Point3D;
            if (Inside(point3D_Move, silverSpacing, tolerance))
                vector3D.Negate();

            return vector3D.Unit;
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
    }
}