using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Architectural
{
    public abstract class BuildingElementType : SAMType, IArchitecturalObject
    {
        public BuildingElementType(BuildingElementType buildingElementType)
            : base(buildingElementType)
        {

        }

        public BuildingElementType(JObject jObject)
            : base(jObject)
        {

        }

        public BuildingElementType(string name)
            : base(name)
        {

        }

        public BuildingElementType(System.Guid guid, string name)
            : base(guid, name)
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
