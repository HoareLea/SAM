using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public class IntegerId : ParameterizedSAMObject
    {
        private int id;

        public IntegerId(int id)
            : base()
        {
            this.id = id;
        }

        public IntegerId(IntegerId integerId)
            :base(integerId)
        {
            id = integerId.id;
        }

        public IntegerId(JObject jObject)
            : base(jObject)
        {

        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (!base.FromJObject(jObject))
            {
                return false;
            }

            id = jObject.Value<int>("Id");
            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return result;
            }

            result.Add("Id", id);

            return result;
        }

        public static implicit operator IntegerId(int id)
        {
            return new IntegerId(id);
        }
    }
}