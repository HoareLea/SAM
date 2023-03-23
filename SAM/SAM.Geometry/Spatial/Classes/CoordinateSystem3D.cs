using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Geometry.Spatial
{
    public class CoordinateSystem3D : IJSAMObject
    {
        private Point3D origin;
        private Vector3D axisX;
        private Vector3D axisY;
        private Vector3D axisZ;

        public CoordinateSystem3D(Point3D origin, Vector3D axisX, Vector3D axisY, Vector3D axisZ)
        {
            this.origin = origin;
            this.axisX = axisX;
            this.axisY = axisY;
            this.axisZ = axisZ;
        }

        public CoordinateSystem3D(Plane plane)
        {
            if(plane != null)
            {
                origin = plane.Origin;
                axisX = plane.AxisX;
                axisY = plane.AxisY;
                axisZ = plane.AxisZ;
            }
        }

        public CoordinateSystem3D(CoordinateSystem3D coordinateSystem3D)
        {
            if(coordinateSystem3D != null)
            {
                origin = coordinateSystem3D.Origin;
                axisX = coordinateSystem3D.AxisX;
                axisY = coordinateSystem3D.AxisX;
                axisZ = coordinateSystem3D.AxisZ;
            }
        }

        public CoordinateSystem3D()
        {
            origin = Point3D.Zero;
            axisX = Vector3D.WorldX;
            axisY = Vector3D.WorldY;
            axisZ = Vector3D.WorldZ;
        }

        public CoordinateSystem3D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Vector3D AxisX
        {
            get
            {
                return axisX == null ? null : new Vector3D(axisX);
            }
        }

        public Vector3D AxisY
        {
            get
            {
                return axisY == null ? null : new Vector3D(axisY);
            }
        }

        public Vector3D AxisZ
        {
            get
            {
                return axisZ == null ? null : new Vector3D(axisZ);
            }
        }

        public Point3D Origin
        {
            get
            {
                return origin == null ? null : new Point3D(origin);
            }
        }

        public bool IsValid()
        {
            return origin != null && axisX != null && axisY != null && axisZ != null && axisX.IsValid() && axisY.IsValid() && axisZ.IsValid() && origin.IsValid();
        }

        public bool FromJObject(JObject jObject)
        {
            if(axisX != null)
            {
                jObject.Add("AxisX", axisX.ToJObject());
            }

            if (axisY != null)
            {
                jObject.Add("AxisY", axisY.ToJObject());
            }

            if (axisZ != null)
            {
                jObject.Add("AxisZ", axisZ.ToJObject());
            }

            if (origin != null)
            {
                jObject.Add("Origin", origin.ToJObject());
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (axisX != null)
            {
                result.Add("AxisX", axisX.ToJObject());
            }

            if (axisY != null)
            {
                result.Add("AxisY", axisY.ToJObject());
            }

            if (axisZ != null)
            {
                result.Add("AxisZ", axisZ.ToJObject());
            }

            if (origin != null)
            {
                result.Add("Origin", origin.ToJObject());
            }

            return result;
        }

        public static CoordinateSystem3D World
        {
            get
            {
                return new CoordinateSystem3D();
            }
        }

    }
}