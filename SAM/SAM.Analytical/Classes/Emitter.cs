using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class Emitter : SAMObject, IAnalyticalObject
    {
        private EmitterCategory emitterCategory;
        private double radiantProportion;
        private double viewCoefficient;

        public Emitter(string name)
            : base(name)
        {
            radiantProportion = double.NaN;
            viewCoefficient = double.NaN;
            emitterCategory = EmitterCategory.Undefined;
        }

        public Emitter(Guid guid, string name)
            : base(guid, name)
        {
            radiantProportion = double.NaN;
            viewCoefficient = double.NaN;
            emitterCategory = EmitterCategory.Undefined;
        }

        public Emitter(Guid guid, string name, EmitterCategory emitterCategory, double radiantProportion, double viewCoefficient)
            : base(guid, name)
        {
            this.radiantProportion = radiantProportion;
            this.viewCoefficient = viewCoefficient;
            this.emitterCategory = emitterCategory;
        }

        public Emitter(Emitter emitter)
            : base(emitter)
        {
            radiantProportion = emitter.radiantProportion;
            viewCoefficient = emitter.viewCoefficient;
            emitterCategory = emitter.emitterCategory;
        }

        public Emitter(JObject jObject)
            : base(jObject)
        {
        }

        public double RadiantProportion
        {
            get
            {
                return radiantProportion;
            }
        }

        public double ViewCoefficient
        {
            get
            {
                return viewCoefficient;
            }
        }

        public EmitterCategory EmitterCategory
        {
            get
            {
                return emitterCategory;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("RadiantProportion"))
                radiantProportion = jObject.Value<double>("RadiantProportion");

            if (jObject.ContainsKey("ViewCoefficient"))
                viewCoefficient = jObject.Value<double>("ViewCoefficient");

            if (jObject.ContainsKey("EmitterCategory"))
                emitterCategory = jObject.Value<string>("EmitterCategory").Enum<EmitterCategory>();

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(!double.IsNaN(radiantProportion))
                jObject.Add("RadiantProportion", radiantProportion);

            if (!double.IsNaN(viewCoefficient))
                jObject.Add("ViewCoefficient", viewCoefficient);

            if (emitterCategory != EmitterCategory.Undefined)
                jObject.Add("EmitterCategory", emitterCategory.ToString());

            return jObject;
        }
    }
}