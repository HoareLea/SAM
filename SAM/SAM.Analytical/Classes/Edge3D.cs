using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Edge3D : SAMObject
    {
        private ICurve3D curve3D;

        public Edge3D(System.Guid guid, string name, ICurve3D curve3D)
            : base(guid, name)
        {
            this.curve3D = (ICurve3D)curve3D.Clone();
        }

        public Edge3D(Plane plane, Edge2D edge2D)
            : base(System.Guid.NewGuid(), edge2D)
        {
            curve3D = (ICurve3D)plane.Convert(edge2D.Curve2D);
        }
        
        public Edge3D(ICurve3D curve)
            : base()
        {
            this.curve3D = (ICurve3D)curve3D.Clone(); ;
        }

        public Edge3D(Edge3D edge)
            : base(edge)
        {
            curve3D = (ICurve3D)edge.curve3D.Clone();
        }

        public Edge3D(JObject jObject)
            : base(jObject)
        {

        }

        public List<Segment3D> ToSegments()
        {
            if (curve3D is ISegmentable3D)
                return ((ISegmentable3D)curve3D).GetSegments();
            return null;
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            if (point3Ds == null)
                return;

            if (curve3D is Segment3D)
            {
                Segment3D segment3D = (Segment3D)curve3D;
                Point3D point3D_1 = Point3D.Closest(point3Ds, segment3D[0]);
                Point3D point3D_2 = Point3D.Closest(point3Ds, segment3D[1]);

                if(!double.IsNaN(maxDistance))
                {
                    if (point3D_1.Distance(segment3D[0]) > maxDistance)
                        point3D_1 = new Point3D(segment3D[0]);

                    if(point3D_2.Distance(segment3D[1]) > maxDistance)
                        point3D_2 = new Point3D(segment3D[1]);
                }

                curve3D = new Segment3D(point3D_1, point3D_2);
            }
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return curve3D.GetBoundingBox(offset);
        }

        public ICurve3D Curve3D
        {
            get
            {
                return (ICurve3D)curve3D.Clone();
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            curve3D = Geometry.Spatial.Create.ICurve3D(jObject.Value<JObject>("Curve3D"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Curve3D", curve3D.ToJObject());
            return jObject;
        }


        public static IEnumerable<Edge3D> FromGeometry(ISAMGeometry3D geometry3D)
        {
            ISAMGeometry3D geometry3D_Temp = geometry3D;

            if (geometry3D is IClosed3D)
                geometry3D_Temp = ((IClosed3D)geometry3D).GetBoundary();

            if (geometry3D_Temp is Polycurve3D)
                return ((Polycurve3D)geometry3D_Temp).Explode().ConvertAll(x => new Edge3D(x));

            if (geometry3D_Temp is ICurvable3D)
            {
                ICurvable3D curvable3D = (ICurvable3D)geometry3D_Temp;
                List<Edge3D> result = new List<Edge3D>();
                foreach (ICurve3D curve3D in curvable3D.GetCurves())
                {
                    if (curve3D is Polycurve3D)
                        result.AddRange(FromGeometry(curve3D));
                    else
                        result.Add(new Edge3D(curve3D));
                }
                return result;

            }

            return null;
        }
    }
}
