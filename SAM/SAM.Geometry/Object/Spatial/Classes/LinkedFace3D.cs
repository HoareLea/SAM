using Newtonsoft.Json.Linq;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry.Object.Spatial
{
    public class LinkedFace3D : Core.IJSAMObject, IFace3DObject
    {
        private Guid guid;
        private BoundingBox3D boundingBox3D;
        private Face3D face3D;

        public Face3D Face3D
        {
            get
            {
                return face3D;
            }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        public LinkedFace3D(Guid guid, Face3D face3D)
        {
            this.guid = guid;
            this.face3D = new Face3D(face3D);
            boundingBox3D = this.face3D?.GetBoundingBox();
        }

        public LinkedFace3D(LinkedFace3D linkedFace3D)
        {
            if (linkedFace3D == null)
            {
                return;
            }

            guid = linkedFace3D.guid;
            if (linkedFace3D.face3D != null)
            {
                face3D = new Face3D(linkedFace3D.face3D);
                boundingBox3D = face3D?.GetBoundingBox();
            }
        }

        public LinkedFace3D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            if (boundingBox3D == null)
            {
                boundingBox3D = face3D?.GetBoundingBox();
            }

            if (boundingBox3D == null)
            {
                return null;
            }

            return new BoundingBox3D(boundingBox3D, offset);
        }

        public void Move(Vector3D vector3D)
        {
            face3D = face3D?.GetMoved(vector3D) as Face3D;
            boundingBox3D = face3D?.GetBoundingBox();
        }

        public void Transform(Transform3D transform3D)
        {
            face3D = face3D?.GetTransformed(transform3D) as Face3D;
            boundingBox3D = face3D?.GetBoundingBox();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (guid != Guid.Empty)
            {
                jObject.Add("Guid", guid);
            }

            if (face3D != null)
            {
                jObject.Add("Face3D", face3D.ToJObject());
            }

            if (boundingBox3D != null)
            {
                jObject.Add("BoundingBox3D", boundingBox3D.ToJObject());
            }

            return jObject;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Guid"))
            {
                guid = Core.Query.Guid(jObject, "Guid");
            }

            if (jObject.ContainsKey("Face3D"))
            {
                face3D = new Face3D(jObject.Value<JObject>("Face3D"));
            }

            if (jObject.ContainsKey("BoundingBox3D"))
            {
                boundingBox3D = new BoundingBox3D(jObject.Value<JObject>("BoundingBox3D"));
            }

            return true;
        }
    }
}
