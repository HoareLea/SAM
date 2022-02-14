using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public abstract class MechanicalSystem : SAMInstance<MechanicalSystemType>, ISystem, IAnalyticalObject
    {
        private string id;

        public MechanicalSystem(string id, MechanicalSystemType mechanicalSystemType)
            : base(mechanicalSystemType)
        {
            this.id = id;
        }

        public MechanicalSystem(MechanicalSystem mechanicalSystem)
            : base(mechanicalSystem)
        {
            id = mechanicalSystem?.id;
        }

        public MechanicalSystem(JObject jObject)
            : base(jObject)
        {
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", name == null ? string.Empty : name, id == null ? string.Empty : id).Trim();
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Id"))
            {
                id = jObject.Value<string>("Id");
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (id != null)
                jObject.Add("Id", id);

            return jObject;
        }
    }
}