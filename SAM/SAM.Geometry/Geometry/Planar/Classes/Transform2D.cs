using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using SAM.Math;

namespace SAM.Geometry.Planar
{
    public class Transform2D : ITransform2D
    {
        private Matrix3D matrix3D;

        public Transform2D(Matrix3D matrix3D)
        {
            this.matrix3D = matrix3D;
        }

        public Transform2D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Transform2D(Transform2D transform2D)
        {
            matrix3D = new Matrix3D(transform2D.matrix3D);
        }

        public Matrix3D Matrix3D
        {
            get
            {
                return new Matrix3D(matrix3D);
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            matrix3D = new Matrix3D(jObject.Value<JObject>("Matrix3D"));
            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("Matrix3D", matrix3D.ToJObject());
            return jObject;
        }

        public static Transform2D GetIdentity()
        {
            return new Transform2D(Matrix3D.GetIdentity());
        }

        public static Transform2D GetScale(double factor)
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 0] = factor;
            result.matrix3D[1, 1] = factor;
            result.matrix3D[2, 2] = factor;

            return result;
        }

        public static Transform2D GetScale(double x, double y)
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 0] = x;
            result.matrix3D[1, 1] = y;

            return result;
        }

        public static Transform2D GetTranslation(Vector2D vector2D)
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 2] = vector2D[0];
            result.matrix3D[1, 2] = vector2D[1];

            return result;
        }

        public static Transform2D GetTranslation(double x, double y)
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 2] = x;
            result.matrix3D[1, 2] = y;

            return result;
        }

        public static Transform2D GetRotation(double angle)
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 0] = System.Math.Cos(angle);
            result.matrix3D[0, 1] = -System.Math.Sin(angle);
            result.matrix3D[1, 0] = System.Math.Sin(angle);
            result.matrix3D[1, 1] = System.Math.Cos(angle);

            return result;
        }

        public static Transform2D GetRotation(Point2D origin, double angle)
        {
            Transform2D transform2D_Translation_1 = GetTranslation(origin.X, origin.Y);
            Transform2D transform2D_Rotation = GetRotation(angle);
            Transform2D transform2D_Translation_2 = GetTranslation(-origin.X, -origin.Y);

            return transform2D_Translation_1 * transform2D_Rotation * transform2D_Translation_2;
        }

        public static Transform2D GetCoordinateSystem2DToOrigin(CoordinateSystem2D coordinateSystem2D)
        {
            if (coordinateSystem2D == null)
            {
                return null;
            }

            Point2D origin = coordinateSystem2D.Origin;
            Vector2D axisX = coordinateSystem2D.AxisX;
            Vector2D axisY = coordinateSystem2D.AxisY;

            Matrix3D matrix3D = Matrix3D.GetIdentity();
            matrix3D[0, 0] = Vector2D.WorldX.DotProduct(axisX);
            matrix3D[0, 1] = Vector2D.WorldX.DotProduct(axisY);
            matrix3D[0, 2] = origin.X;

            matrix3D[1, 0] = Vector2D.WorldY.DotProduct(axisX);
            matrix3D[1, 1] = Vector2D.WorldY.DotProduct(axisY);
            matrix3D[1, 2] = origin.Y;

            return new Transform2D(matrix3D);
        }

        public static Transform2D GetMirrorX()
        {
            Transform2D result = GetIdentity();
            result.matrix3D[1, 1] = -result.matrix3D[1, 1];

            return result;
        }

        public static ITransform2D GetMirrorX(Point2D point2D)
        {
            if(point2D == null)
            {
                return null;
            }

            CoordinateSystem2D coordinateSystem2D = new CoordinateSystem2D(point2D);

            return new TransformGroup2D(new Transform2D[] { GetOriginToCoordinateSystem2D(coordinateSystem2D), GetMirrorX(), GetCoordinateSystem2DToOrigin(coordinateSystem2D) });
        }

        public static Transform2D GetMirrorY()
        {
            Transform2D result = GetIdentity();
            result.matrix3D[0, 0] = -result.matrix3D[0, 0];

            return result;
        }

        public static ITransform2D GetMirrorY(Point2D point2D)
        {
            if (point2D == null)
            {
                return null;
            }

            CoordinateSystem2D coordinateSystem2D = new CoordinateSystem2D(point2D);

            return new TransformGroup2D(new Transform2D[] { GetOriginToCoordinateSystem2D(coordinateSystem2D), GetMirrorY(), GetCoordinateSystem2DToOrigin(coordinateSystem2D) });
        }

        public static Transform2D GetOriginToCoordinateSystem2D(CoordinateSystem2D coordinateSystem2D)
        {
            Transform2D result = GetCoordinateSystem2DToOrigin(coordinateSystem2D);
            result.Inverse();
            return result;
        }

        public static Transform2D GetCoordinateSystem2DToCoordinateSystem2D(CoordinateSystem2D coordinateSystem2D_From, CoordinateSystem2D coordinateSystem2D_To)
        {
            Transform2D transform2D_From = GetCoordinateSystem2DToOrigin(coordinateSystem2D_From);
            Transform2D transform2D_To = GetOriginToCoordinateSystem2D(coordinateSystem2D_To);

            return transform2D_To * transform2D_From;
        }

        public void Inverse()
        {
            matrix3D?.Inverse();
        }

        public void Transpose()
        {
            matrix3D?.Transpose();
        }

        public static Transform2D operator *(Transform2D transform2D_1, Transform2D transform2D_2)
        {
            if (transform2D_1 == null || transform2D_2 == null)
                return null;

            return new Transform2D(transform2D_1.matrix3D * transform2D_2.matrix3D);
        }
    }
}