using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry
{
    public abstract class SAMGeometry : ISAMGeometry
    {
        public SAMGeometry()
        {

        }
        
        public SAMGeometry(JObject jObject)
        {
            FromJObject(jObject);
        }

        public abstract ISAMGeometry Clone();

        public abstract bool FromJObject(JObject jObject);

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.TypeName(this));
            return jObject;
        }
    }
}
