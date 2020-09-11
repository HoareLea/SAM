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

        public FluidMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeat, double dynamicViscosity)
        : base(guid, name, displayName, description, thermalConductivity, density, specificHeat)
        {
            this.dynamicViscosity = dynamicViscosity;
        }

        public FluidMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeat, double density, double dynamicViscosity)
            : base(name, group, displayName, description, thermalConductivity, specificHeat, density)
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