using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class BoundaryEdge2DLoop : SAMObject
    {
        private List<BoundaryEdge2D> boundaryEdge2Ds;

        public BoundaryEdge2DLoop(System.Guid guid, string name, IEnumerable<ParameterSet> parameterSets, IEnumerable<BoundaryEdge2D> boundaryEdge2Ds)
            : base(guid, name, parameterSets)
        {
            if (boundaryEdge2Ds != null)
                this.boundaryEdge2Ds = boundaryEdge2Ds.ToList().ConvertAll(x => new BoundaryEdge2D(x));
        }

        public BoundaryEdge2DLoop(System.Guid guid, BoundaryEdge2DLoop boundaryEdge2DLoop)
            : base(guid, boundaryEdge2DLoop)
        {
            if (boundaryEdge2DLoop != null)
                boundaryEdge2Ds = boundaryEdge2DLoop.boundaryEdge2Ds.ToList().ConvertAll(x => new BoundaryEdge2D(x));
        }

        public BoundaryEdge2DLoop(Geometry.Spatial.Plane plane, BoundaryEdge3DLoop boundaryEdge3DLoop)
            : base(System.Guid.NewGuid(), boundaryEdge3DLoop)
        {
            boundaryEdge2Ds = boundaryEdge3DLoop.BoundaryEdge3Ds.ConvertAll(x => new BoundaryEdge2D(plane, x));
        }

        public BoundaryEdge2DLoop(Geometry.Planar.IClosed2D closed2D)
            : base()
        {
            boundaryEdge2Ds = BoundaryEdge2D.FromGeometry(closed2D).ToList();
        }

        public BoundaryEdge2DLoop(Geometry.Spatial.IClosedPlanar3D closedPlanar3D)
        {
            IEnumerable<BoundaryEdge2D> boundaryEdge2Ds_Temp = BoundaryEdge2D.FromGeometry(closedPlanar3D);
            if (boundaryEdge2Ds_Temp != null)
                boundaryEdge2Ds = new List<BoundaryEdge2D>(boundaryEdge2Ds_Temp);
        }

        public BoundaryEdge2DLoop(Geometry.Spatial.Face3D face)
        {
            boundaryEdge2Ds = BoundaryEdge2D.FromGeometry(face.ExternalEdge).ToList();
        }

        public BoundaryEdge2DLoop(BoundaryEdge2DLoop boundaryEdge2DLoop)
            : base(boundaryEdge2DLoop)
        {
            this.boundaryEdge2Ds = boundaryEdge2DLoop.boundaryEdge2Ds.ConvertAll(x => new BoundaryEdge2D(x));
        }

        public BoundaryEdge2DLoop(JObject jObject)
            : base(jObject)
        {
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            if (boundaryEdge2Ds == null || boundaryEdge2Ds.Count == 0)
                return null;

            return new BoundingBox2D(boundaryEdge2Ds.ConvertAll(x => x.Curve2D.GetBoundingBox(offset)));
        }

        public List<BoundaryEdge2D> BoundaryEdge2Ds
        {
            get
            {
                return boundaryEdge2Ds.ConvertAll(x => new BoundaryEdge2D(x));
            }
        }

        public Geometry.Planar.IClosed2D GetClosed2D()
        {
            return ToGeometry(this);
        }

        public double GetArea()
        {
            return GetClosed2D().GetArea();
        }

        public double GetPerimeter()
        {
            if (boundaryEdge2Ds == null)
                return double.NaN;

            double perimeter = 0;
            foreach(BoundaryEdge2D boundaryEdge2D in boundaryEdge2Ds)
            {
                SAM.Geometry.Planar.ICurve2D curve2D = boundaryEdge2D.Curve2D;
                if (curve2D == null)
                    continue;

                perimeter += curve2D.GetLength();
            }

            return perimeter;
        }

        public bool Move(Geometry.Planar.Vector2D vector2D)
        {
            if (boundaryEdge2Ds == null || vector2D == null)
                return false;

            boundaryEdge2Ds.ForEach(x => x.Move(vector2D));
            return true;
        }

        public void Reverse()
        {
            foreach (BoundaryEdge2D boundaryEdge2D in boundaryEdge2Ds)
                boundaryEdge2D.Reverse();

            boundaryEdge2Ds.Reverse();
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            boundaryEdge2Ds = Core.Create.IJSAMObjects<BoundaryEdge2D>(jObject.Value<JArray>("BoundaryEdge2Ds"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("BoundaryEdge2Ds", Core.Create.JArray(boundaryEdge2Ds));
            return jObject;
        }

        public static Geometry.Planar.IClosed2D ToGeometry(BoundaryEdge2DLoop boundaryEdge2DLoop)
        {
            List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
            foreach (BoundaryEdge2D edge2D in boundaryEdge2DLoop.boundaryEdge2Ds)
            {
                Geometry.Planar.Segment2D segment2D = edge2D.Curve2D as Geometry.Planar.Segment2D;
                point2Ds.Add(segment2D.GetStart());
            }
            return new Geometry.Planar.Polygon2D(point2Ds);
        }
    }
}