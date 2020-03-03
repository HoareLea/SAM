using System;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Line2D : SAMGeometry, ICurve2D
    {
        
        public Line2D(JObject jObject)
            : base(jObject)
        {

        }
        
        public override ISAMGeometry Clone()
        {
            throw new NotImplementedException();
        }

        public override bool FromJObject(JObject jObject)
        {
            throw new NotImplementedException();
        }

        public Point2D GetEnd()
        {
            return new Point2D(double.MaxValue, double.MaxValue);
        }

        public double GetLength()
        {
            return double.MaxValue;
        }

        public Point2D GetStart()
        {
            return new Point2D(double.MinValue, double.MinValue);
        }

        public void Reverse()
        {
            throw new NotImplementedException();
        }

        public override JObject ToJObject()
        {
            throw new NotImplementedException();
        }
    }
}
