using System;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Line2D : SAMGeometry
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

        public override JObject ToJObject()
        {
            throw new NotImplementedException();
        }
    }
}
