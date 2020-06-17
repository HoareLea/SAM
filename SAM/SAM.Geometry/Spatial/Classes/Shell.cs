using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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

            if (!boundingBox3D.Inside(point3D))
                return false;

            Vector3D vector3D = new Vector3D(boundingBox3D.Min, boundingBox3D.Max);
            if (vector3D.Length < silverSpacing)
                return false;

            Segment3D segment3D = new Segment3D(point3D, vector3D);

            HashSet<Point3D> point3Ds = new HashSet<Point3D>();
            foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(boundary.Item2, segment3D, tolerance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Point3D> point3Ds_Temp = planarIntersectionResult.GetGeometry3Ds<Point3D>();
                if (point3Ds_Temp != null && point3Ds_Temp.Count > 0)
                    point3Ds_Temp.ForEach(x => point3Ds.Add(x));
            }

            if (point3Ds == null || point3Ds.Count == 0)
                return false;

            return point3Ds.Count % 2 != 0;
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null || boundaries == null || boundingBox3D == null)
                return false;

            if (!boundingBox3D.Inside(point3D, true, tolerance))
                return false;

            foreach(Tuple<BoundingBox3D, Face3D> boundary in boundaries)
            {
                if (!boundary.Item1.Inside(point3D, true, tolerance))
                    continue;

                if (boundary.Item2.On(point3D))
                    return true;
            }

            return false;
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

            if(jObject.ContainsKey("Face3Ds"))
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
    }
}