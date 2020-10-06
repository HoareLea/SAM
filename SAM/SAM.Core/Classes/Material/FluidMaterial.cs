using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public abstract class FluidMaterial : Material
    {
        private double dynamicViscosity = double.NaN;
        
        public FluidMaterial(string name)
            : base(name)
        {

        }

        public FluidMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeatCapacity, double dynamicViscosity)
        : base(guid, name, displayName, description, thermalConductivity, density, specificHeatCapacity)
        {
            this.dynamicViscosity = dynamicViscosity;
        }

        public FluidMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density, double dynamicViscosity)
            : base(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density)
        {
            this.dynamicViscosity = dynamicViscosity;
        }

        public FluidMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public FluidMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public FluidMaterial(FluidMaterial fluidMaterial)
            : base(fluidMaterial)
        {
            dynamicViscosity = fluidMaterial.dynamicViscosity;
        }

        public FluidMaterial(string name, Guid guid, FluidMaterial fluidMaterial, string displayName, string description)
            : base(name, guid, fluidMaterial, displayName, description)
        {
            dynamicViscosity = fluidMaterial.dynamicViscosity;
        }

        /// <summary>
        ///  Dynamic Viscosity of Fluid [kg/(m*s)]
        /// </summary>
        public double DynamicViscosity
        {
            get
            {
                return dynamicViscosity;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("DynamicViscosity"))
                dynamicViscosity = jObject.Value<double>("DynamicViscosity");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (!double.IsNaN(dynamicViscosity))
                jObject.Add("DynamicViscosity", dynamicViscosity);

            return jObject;
        }
    }
}