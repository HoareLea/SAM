using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Geometry.Planar
{
    public class CoordinateSystem2D : IJSAMObject
    {
        private Point2D origin;
        private Vector2D axisX;
        private Vector2D axisY;

        public CoordinateSystem2D(Point2D origin, Vector2D axisX, Vector2D axisY)
        {
            this.origin = origin == null ? null : new Point2D(origin);
            this.axisX = axisX == null ? null : new Vector2D(axisX);
            this.axisY = axisY == null ? null : new Vector2D(axisY);
        }

        public CoordinateSystem2D(Point2D origin)
        {
            this.origin = origin == null ? null : new Point2D(origin);
            axisX = Vector2D.WorldX;
            axisY = Vector2D.WorldY;
        }

        public CoordinateSystem2D()
        {
            origin = Point2D.Zero;
            axisX = Vector2D.WorldX;
            axisY = Vector2D.WorldY;
        }

        public CoordinateSystem2D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public CoordinateSystem2D(CoordinateSystem2D coordinateSystem2D)
        {
            if(coordinateSystem2D != null)
            {
                origin = coordinateSystem2D.origin;
                axisX = coordinateSystem2D.axisX;
                axisY = coordinateSystem2D.axisY;
            }
        }

        public Vector2D AxisX
        {
            get
            {
                return axisX == null ? null : new Vector2D(axisX);
            }
        }

        public Vector2D AxisY
        {
            get
            {
                return axisY == null ? null : new Vector2D(axisY);
            }
        }

        public Point2D Origin
        {
            get
            {
                return origin == null ? null : new Point2D(origin);
            }
        }

        public CoordinateSystem2D GetMoved(Vector2D vector2D)
        {
            return new CoordinateSystem2D(origin.GetMoved(vector2D), axisX, axisY);
        }

        public bool Move(Vector2D vector2D)
        {
            if(origin == null || vector2D == null)
            {
                return false;
            }

            origin.Move(vector2D);

            return true;
        }

        public bool IsValid()
        {
            return origin != null && axisX != null && axisY != null && axisX.IsValid() && axisY.IsValid() && origin.IsValid();
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("AxisX"))
            {
                axisX = new Vector2D(jObject.Value<JObject>("AxisX"));
            }

            if (jObject.ContainsKey("AxisY"))
            {
                axisY = new Vector2D(jObject.Value<JObject>("AxisY"));
            }

            if (jObject.ContainsKey("Origin"))
            {
                origin = new Point2D(jObject.Value<JObject>("Origin"));
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

            if (origin != null)
            {
                result.Add("Origin", origin.ToJObject());
            }

            return result;
        }

        public CoordinateSystem2D GetTransformed(Transform2D transform2D)
        {
            if(transform2D == null)
            {
                return null;
            }

            Point2D origin_New = Query.Transform(origin, transform2D);
            Vector2D axisX_New = Query.Transform(axisX, transform2D);
            Vector2D axisY_New = Query.Transform(axisY, transform2D);

            return new CoordinateSystem2D(origin_New, axisX_New, axisY_New);
        }

        public CoordinateSystem2D Clone()
        {
            return new CoordinateSystem2D(origin, axisX, axisY);
        }

        public static CoordinateSystem2D World
        {
            get
            {
                return new CoordinateSystem2D();
            }
        }

    }
}