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

        public Matrix4D Matrix4D
        {
            get
            {
                return new Matrix4D(matrix4D);
            }
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

        public static Transform3D GetIdentity()
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
            return new Transform3D(Matrix4D.GetUnset());
        }

        public static Transform3D GetScale(double factor)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = factor;
            result.matrix4D[1, 1] = factor;
            result.matrix4D[2, 2] = factor;

            return result;
        }

        public static Transform3D GetScale(double x, double y, double z)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = x;
            result.matrix4D[1, 1] = y;
            result.matrix4D[2, 2] = z;

            return result;
        }

        public static Transform3D GetTranslation(Vector3D vector3D)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 3] = vector3D[0];
            result.matrix4D[1, 3] = vector3D[1];
            result.matrix4D[2, 3] = vector3D[2];

            return result;
        }

        public static Transform3D GetTranslation(double x, double y, double z)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 3] = x;
            result.matrix4D[1, 3] = y;
            result.matrix4D[2, 3] = z;

            return result;
        }
    }
}