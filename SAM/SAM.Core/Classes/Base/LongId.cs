using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class LongId : ParameterizedSAMObject, IId
    {
        private long id;

        public LongId(long id)
            : base()
        {
            this.id = id;
        }

        public LongId(int id)
            : base()
        {
            this.id = id;
        }

        public LongId(LongId longId)
            :base(longId)
        {
            id = longId.id;
        }

        public LongId(JObject jObject)
            : base(jObject)
        {

        }

        public long Id
        {
            get
            {
                return id;
            }
        }

        public static implicit operator LongId(long id)
        {
            return new LongId(id);
        }

        public static implicit operator LongId(int id)
        {
            return new LongId(System.Convert.ToInt64(id));
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (!base.FromJObject(jObject))
            {
                return false;
            }

            id = jObject.Value<long>("Id");
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
    }
}