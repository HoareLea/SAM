using Newtonsoft.Json.Linq;

namespace SAM.Architectural
{
    public abstract class PartitionType : BuildingElementType
    {
        public PartitionType(PartitionType partitionType)
            : base(partitionType)
        {

        }

        public PartitionType(JObject jObject)
            : base(jObject)
        {

        }

        public PartitionType(string name)
            : base(name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            return jObject;
        }

    }
}
