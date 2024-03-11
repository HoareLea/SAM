using Newtonsoft.Json.Linq;
using SAM.Math;
using System;

namespace SAM.Geometry.Planar
{
    public class Line2D : SAMGeometry, ISAMGeometry2D
    {
        private Point2D origin;
        private Vector2D vector;

        public Line2D(JObject jObject)
            : base(jObject)
        {
        }

        public Line2D(Point2D origin, Vector2D vector)
        {
            this.origin = origin;
            this.vector = vector;
        }

        public Line2D(Line2D line2D)
        {
            origin = new Point2D(line2D.origin);
            vector = new Vector2D(line2D.vector);
        }

        public Vector2D Direction
        {
            get
            {
                return new Vector2D(vector);
            }
        }

        public Point2D Origin
        {
            get
            {
                return new Point2D(origin);
            }
        }

        public static explicit operator Line2D(Segment2D segment2D)
        {
            if (segment2D == null)
                return null;

            return new Line2D(segment2D[0], segment2D.Direction);
        }

        public static explicit operator Line2D(LinearEquation linearEquation)
        {
            if (linearEquation == null)
            {
                return null;
            }

            double x_1 = 0;
            double y_1 = linearEquation.Evaluate(x_1);
            if (double.IsNaN(y_1))
            {
                return null;
            }

            double x_2 = 1;
            double y_2 = linearEquation.Evaluate(x_2);
            if (double.IsNaN(y_2))
            {
                return null;
            }

            Point2D point_1 = new Point2D(x_1, y_1);
            Point2D point_2 = new Point2D(x_2, y_2);

            return new Line2D(point_1, new Vector2D(point_1, point_2));
        }

        public override ISAMGeometry Clone()
        {
            return new Line2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point2D(jObject.Value<JObject>("Origin"));
            vector = new Vector2D(jObject.Value<JObject>("Vector"));

            return true;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(origin, vector).GetHashCode();
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }

        /// <summary>
        /// Changes line2D to string formula. Source: https://math.stackexchange.com/questions/404440/what-is-the-equation-for-a-3d-line
        /// </summary>
        /// <param name="lineFormulaForm">LineFormulaForm</param>
        /// <returns>string</returns>
        public string ToString(LineFormulaForm lineFormulaForm)
        {
            if (lineFormulaForm == LineFormulaForm.Undefined)
                return null;

            Vector2D direction = vector.Unit;

            switch (lineFormulaForm)
            {
                case LineFormulaForm.Parameteric:

                    return string.Format("x={0}{1}\ny={2}{3}", origin.X, Core.Convert.ToString(direction.X, "t"), origin.Y, Core.Convert.ToString(direction.Y, "t"));

                case LineFormulaForm.Vector:
                    return string.Format("(x;y)=({0};{1}})+t({2};{3})", origin.X, origin.Y, direction.X, direction.Y);

                case LineFormulaForm.Symmetric:
                    return string.Format("(x{0})/{1}=(y{2})/{3}", -origin.X, direction.X, -origin.Y, direction.X);
            }

            return null;
        }

        public Point2D Intersection(Line2D line2D, double tolerance = Core.Tolerance.Distance)
        {
            if(line2D == null)
            {
                return null;
            }

            return Query.Intersection(origin, origin.GetMoved(vector), line2D.origin, line2D.origin.GetMoved(line2D.vector), false, tolerance);
        }

        public Point2D Intersection(Segment2D segment2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null)
            {
                return null;
            }

            Point2D point2D =  Query.Intersection(origin, origin.GetMoved(vector), segment2D.Start, segment2D.End, false, tolerance);
            if(point2D == null)
            {
                return null;
            }

            return segment2D.On(point2D, tolerance) ? point2D : null;

        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            Point2D origin_New = Query.Transform(origin, transform2D);
            Vector2D vector_New = Query.Transform(vector, transform2D);

            return new Line2D(origin_New, vector_New);
        }

        public bool Transform(Transform2D transform2D)
        {
            if(transform2D == null)
            {
                return false;
            }

            origin = Query.Transform(origin, transform2D);
            vector = Query.Transform(vector, transform2D);
            return true;

        }
    }
}