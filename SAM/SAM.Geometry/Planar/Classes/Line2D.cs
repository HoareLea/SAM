using Newtonsoft.Json.Linq;
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

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(origin, vector).GetHashCode();
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
    }
}