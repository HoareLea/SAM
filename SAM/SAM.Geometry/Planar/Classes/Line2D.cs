using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override JObject ToJObject()
        {
            throw new NotImplementedException();
        }
    }
}
