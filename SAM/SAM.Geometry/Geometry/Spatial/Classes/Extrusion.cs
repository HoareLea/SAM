using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Spatial
{
    public class Extrusion : SAMGeometry, IBoundable3D
    {
        private Face3D face3D;
        private Vector3D vector;

        public Extrusion(Face3D face3D, double height)
        {
            this.face3D = new Face3D(face3D);
            vector = new Vector3D(0, 0, height);
        }

        public Extrusion(Face3D face3D, Vector3D vector)
        {
            this.face3D = face3D;
            this.vector = vector;
        }

        public Extrusion(Extrusion extrusion)
        {
            face3D = new Face3D(extrusion.face3D);
            vector = new Vector3D(extrusion.vector);
        }

        public Extrusion(JObject jObject)
            : base(jObject)
        {
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            BoundingBox3D boundingBox3D_1 = face3D?.GetBoundingBox(offset);
            if(boundingBox3D_1 == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_2 = boundingBox3D_1.GetMoved(vector) as BoundingBox3D;
            if(boundingBox3D_2 == null)
            {
                return boundingBox3D_1;
            }

            return new BoundingBox3D(new BoundingBox3D[] { boundingBox3D_1, boundingBox3D_2});
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Extrusion((Face3D)face3D.GetMoved(vector3D), (Vector3D)vector.Clone());
        }

        public Vector3D Vector
        {
            get
            {
                return new Vector3D(vector);
            }
        }

        public Face3D Face3D
        {
            get
            {
                if (face3D == null)
                {
                    return null;
                }

                return new Face3D(face3D);
            }
        }

        public override ISAMGeometry Clone()
        {
            return new Extrusion(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            face3D = new Face3D(jObject.Value<JObject>("Face"));
            vector = new Vector3D(jObject.Value<JObject>("Vector"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Face", face3D.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }
    }
}