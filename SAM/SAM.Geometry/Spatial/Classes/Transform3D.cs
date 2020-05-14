using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Transform3D : IJSAMObject
    {
        private Matrix4D matrix4D;

        public Transform3D(Matrix4D matrix4D)
        {
            this.matrix4D = matrix4D;
        }

        public Transform3D(JObject jObject)
        {
            FromJObject(jObject);
        }
        
        public Transform3D(Transform3D transform3D)
        {
            matrix4D = new Matrix4D(transform3D.matrix4D);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            matrix4D = new Matrix4D(jObject.Value<JObject>("Matrix4D"));
            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("Matrix4D", matrix4D.ToJObject());
            return jObject;
        }

        public static Transform3D GetIndentity()
        {
            return new Transform3D(Matrix4D.GetIdentity());
        }

        /// <summary>Zero Transform3D diagonal = (0,0,0,1)</summary>
        public static Transform3D GetZero()
        {
            Matrix4D matrix4D = new Matrix4D();
            matrix4D[3, 3] = 1;
            return new Transform3D(matrix4D);
        }

        public static Transform3D GetUnset()
        {

        }
    }
}