using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Math;
using System.Collections.Generic;

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

        public Transform3D Multiply(Transform3D transform3D)
        {
            return this * transform3D;
        }

        public Transform3D Multiply(IEnumerable<Transform3D> transform3Ds)
        {
            if (transform3Ds == null)
                return null;

            Transform3D result = this;
            foreach (Transform3D transform3D in transform3Ds)
            {
                result = result * transform3D;

                if (result == null)
                    return null;
            }

            return result;
        }

        public void Inverse()
        {
            matrix4D?.Inverse();
        }

        public void Transpose()
        {
            matrix4D?.Transpose();
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

        public static Transform3D GetScale(Point3D origin, double factor)
        {
            Transform3D transform3D_Translation_1 = GetTranslation(origin.X, origin.Y, origin.Z);
            Transform3D transform3D_Scale = GetScale(factor);
            Transform3D transform3D_Translation_2 = GetTranslation(-origin.X, -origin.Y, -origin.Z);

            return transform3D_Translation_1 * transform3D_Scale * transform3D_Translation_2;
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

        public static Transform3D GetMirrorYZ()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = -result.matrix4D[0, 0];

            return result;
        }

        public static Transform3D GetMirrorXZ()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[1, 1] = -result.matrix4D[1, 1];

            return result;
        }

        public static Transform3D GetMirrorXY()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[2, 2] = -result.matrix4D[2, 2];

            return result;
        }

        public static Transform3D GetProjectionYZ()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = 0;

            return result;
        }

        public static Transform3D GetProjectionXZ()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[1, 1] = 0;

            return result;
        }

        public static Transform3D GetProjectionXY()
        {
            Transform3D result = GetIdentity();
            result.matrix4D[2, 2] = 0;

            return result;
        }

        public static Transform3D GetPlaneToOrigin(Plane plane)
        {
            Transform3D result = GetOriginToPlane(plane);
            result.Inverse();
            return result;
        }

        public static Transform3D GetOriginToPlane(Plane plane)
        {
            if (plane == null)
                return null;

            Point3D origin = plane.Origin;

            Vector3D Vector3D_X = plane.AxisX;
            Vector3D Vector3D_Y = plane.AxisY;
            Vector3D Vector3D_Z = plane.AxisZ;

            Matrix4D matrix4D = Matrix4D.GetIdentity();
            matrix4D[0, 0] = Vector3D.WorldX.DotProduct(Vector3D_X);
            matrix4D[0, 1] = Vector3D.WorldX.DotProduct(Vector3D_Y);
            matrix4D[0, 2] = Vector3D.WorldX.DotProduct(Vector3D_Z);
            matrix4D[0, 3] = origin.X;

            matrix4D[1, 0] = Vector3D.WorldY.DotProduct(Vector3D_X);
            matrix4D[1, 1] = Vector3D.WorldY.DotProduct(Vector3D_Y);
            matrix4D[1, 2] = Vector3D.WorldY.DotProduct(Vector3D_Z);
            matrix4D[1, 3] = origin.Y;

            matrix4D[2, 0] = Vector3D.WorldZ.DotProduct(Vector3D_X);
            matrix4D[2, 1] = Vector3D.WorldZ.DotProduct(Vector3D_Y);
            matrix4D[2, 2] = Vector3D.WorldZ.DotProduct(Vector3D_Z);
            matrix4D[2, 3] = origin.Z;

            return new Transform3D(matrix4D);
        }

        public static Transform3D GetPlaneToPlane(Plane plane_From, Plane plane_To)
        {
            Transform3D transform3D_From = GetPlaneToOrigin(plane_From);
            Transform3D transform3D_To = GetOriginToPlane(plane_To);

            return transform3D_To * transform3D_From;
        }

        /// <summary>
        /// Gets Rotation Transform3D around the z-axis
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Transform3D</returns>
        public static Transform3D GetRotationZ(double angle)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = System.Math.Cos(angle);
            result.matrix4D[0, 1] = -System.Math.Sin(angle);
            result.matrix4D[1, 0] = System.Math.Sin(angle);
            result.matrix4D[1, 1] = System.Math.Cos(angle);

            return result;
        }

        /// <summary>
        /// Gets Rotation Transform3D around the x-axis
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Transform3D</returns>
        public static Transform3D GetRotationX(double angle)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[1, 1] = System.Math.Cos(angle);
            result.matrix4D[1, 2] = -System.Math.Sin(angle);
            result.matrix4D[2, 1] = System.Math.Sin(angle);
            result.matrix4D[2, 2] = System.Math.Cos(angle);

            return result;
        }

        /// <summary>
        /// Gets Rotation Transform3D around the y-axis
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Transform3D</returns>
        public static Transform3D GetRotationY(double angle)
        {
            Transform3D result = GetIdentity();
            result.matrix4D[0, 0] = System.Math.Cos(angle);
            result.matrix4D[0, 2] = System.Math.Sin(angle);
            result.matrix4D[2, 0] = -System.Math.Sin(angle);
            result.matrix4D[2, 2] = System.Math.Cos(angle);

            return result;
        }

        /// <summary>
        /// Rotation Transform3D around the axis. Method to be revised
        /// </summary>
        /// <param name="axis">rotation axis Vector3D</param>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Transform3D</returns>
        public static Transform3D GetRotation(Vector3D axis, double angle)
        {
            //TODO: Revise this method

            if (axis == null)
                return null;

            Transform3D result = GetIdentity();

            Vector3D axis_Unit = axis.Unit;

            result.matrix4D[0, 0] = System.Math.Cos(angle) + System.Math.Pow(axis_Unit.X, 2) * (1 - System.Math.Cos(angle));
            result.matrix4D[1, 0] = axis_Unit.X * axis_Unit.Y * (1 - System.Math.Cos(angle)) - axis_Unit.Z * System.Math.Sin(angle);
            result.matrix4D[2, 0] = axis_Unit.X * axis_Unit.Z * (1 - System.Math.Cos(angle)) - axis_Unit.Y * System.Math.Sin(angle);

            result.matrix4D[0, 1] = axis_Unit.X * axis_Unit.Y * (1 - System.Math.Cos(angle)) + axis_Unit.Z * System.Math.Sin(angle);
            result.matrix4D[1, 1] = System.Math.Cos(angle) + System.Math.Pow(axis_Unit.Y, 2) * (1 - System.Math.Cos(angle));
            result.matrix4D[2, 1] = axis_Unit.Y * axis_Unit.Z * (1 - System.Math.Cos(angle)) - axis_Unit.X * System.Math.Sin(angle);

            result.matrix4D[0, 2] = axis_Unit.X * axis_Unit.Z * (1 - System.Math.Cos(angle)) - axis_Unit.Y * System.Math.Sin(angle);
            result.matrix4D[1, 2] = axis_Unit.Y * axis_Unit.Z * (1 - System.Math.Cos(angle)) + axis_Unit.X * System.Math.Sin(angle);
            result.matrix4D[2, 2] = System.Math.Cos(angle) + System.Math.Pow(axis_Unit.Z, 2) * (1 - System.Math.Cos(angle));

            return result;
        }

        /// <summary>
        /// Rotation Transform around given axis and origin by angle. Method to be revised
        /// </summary>
        /// <param name="origin">Origin Point</param>
        /// <param name="axis">Axis</param>
        /// <param name="angle">Angle</param>
        /// <returns></returns>
        public static Transform3D GetRotation(Point3D origin, Vector3D axis, double angle)
        {
            //TODO: Revise this method

            if (origin == null)
                return null;

            Transform3D transform3D_Translation_1 = GetTranslation(origin.X, origin.Y, origin.Z);
            Transform3D transform3D_Rotation = GetRotation(axis, angle);
            Transform3D transform3D_Translation_2 = GetTranslation(-origin.X, -origin.Y, -origin.Z);

            return transform3D_Translation_1 * transform3D_Rotation * transform3D_Translation_2;
        }

        public static Transform3D operator *(Transform3D transform3D_1, Transform3D transform3D_2)
        {
            if (transform3D_1 == null || transform3D_2 == null)
                return null;

            return new Transform3D(transform3D_1.matrix4D * transform3D_2.matrix4D);
        }
    }
}