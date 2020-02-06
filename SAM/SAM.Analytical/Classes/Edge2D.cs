using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Planar;


namespace SAM.Analytical
{
    public class Edge2D : SAMObject
    {
        private ICurve2D curve2D;

        public Edge2D(Geometry.Spatial.Plane plane, Edge3D edge3D)
            : base(System.Guid.NewGuid(), edge3D)
        {
            curve2D = (ICurve2D)plane.Convert(edge3D.Curve3D);
        }
        
        public Edge2D(Edge2D edge2D)
            : base(edge2D)
        {
            this.curve2D = (ICurve2D)edge2D.curve2D.Clone();
        }

        public Edge2D(ICurve2D curve2D)
            : base()
        {
            this.curve2D = (ICurve2D)curve2D.Clone();
        }

        public Edge2D(System.Guid guid, string name, ICurve2D curve2D)
            : base(guid, name)
        {
            this.curve2D = (ICurve2D)curve2D.Clone();
        }

        public Edge2D(JObject jObject)
            : base(jObject)
        {

        }

        public ICurve2D Curve2D
        {
            get
            {
                return (ICurve2D)curve2D.Clone();
            }
        }

        public void Reverse()
        {
            if(curve2D is Segment2D)
                ((Segment2D)curve2D).Reverse();
        }

        public static IEnumerable<Edge2D> FromGeometry(ISAMGeometry2D geometry2D)
        {
            if (geometry2D is IClosed2D && geometry2D is ISegmentable2D)
                return ((ISegmentable2D)geometry2D).GetSegments().ConvertAll(x => new Edge2D(x));

            return null;
        }

        public static IEnumerable<Edge2D> FromGeometry(Geometry.Spatial.ISAMGeometry3D geometry3D)
        {
            if (geometry3D is Geometry.Spatial.IClosedPlanar3D && geometry3D is Geometry.Spatial.ISegmentable3D)
            {
                Geometry.Spatial.Plane plane = ((Geometry.Spatial.IClosedPlanar3D)geometry3D).GetPlane();
                return ((Geometry.Spatial.ISegmentable3D)geometry3D).GetSegments().ConvertAll(x => new Edge2D(plane.Convert(x)));

            }
                
            return null;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            curve2D = Geometry.Planar.Create.ICurve2D(jObject.Value<JObject>("Curve2D"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Curve2D", curve2D.ToJObject());
            return jObject;
        }
    }
}
