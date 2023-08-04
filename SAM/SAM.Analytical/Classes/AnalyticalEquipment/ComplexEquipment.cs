using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an heat humidifier unit unit object in the analytical domain
    /// </summary>
    public abstract class ComplexEquipment : SAMObject, IComplexEquipment
    {
        protected ComplexEquipmentModel complexEquipmentModel;

        public ComplexEquipment(string name)
            : base(name)
        {

        }

        public ComplexEquipment(JObject jObject)
            : base(jObject)
        {

        }

        public ComplexEquipment(ComplexEquipment complexEquipment)
            : base(complexEquipment)
        {

        }

        public ComplexEquipment(Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("ComplexEquipmentModel"))
            {
                complexEquipmentModel = Core.Query.IJSAMObject<ComplexEquipmentModel>(jObject.Value<JObject>("ComplexEquipmentModel"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(complexEquipmentModel != null)
            {
                jObject.Add("ComplexEquipmentModel", complexEquipmentModel.ToJObject());
            }

            return jObject;
        }
    }
}
