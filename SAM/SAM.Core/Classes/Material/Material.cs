using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public abstract class Material : SAMObject, IMaterial
    {
        private string group;
        private string displayName;
        private string description;

        private double thermalConductivity = double.NaN;
        private double specificHeatCapacity = double.NaN;
        private double density = double.NaN;
        
        public Material(string name)
            : base(name)
        {

        }

        public Material(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density)
            : base(name)
        {
            this.group = group;
            this.displayName = displayName;
            this.description = description;

            this.thermalConductivity = thermalConductivity;
            this.specificHeatCapacity = specificHeatCapacity;
            this.density = density;
        }

        public Material(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeatCapacity)
            :base(guid, name)
        {
            this.displayName = displayName;
            this.description = description;
            this.thermalConductivity = thermalConductivity;
            this.density = density;
            this.specificHeatCapacity = specificHeatCapacity;
        }

        public Material(Guid guid, string name, string group, string displayName, string description)
        : base(guid, name)
        {
            this.group = group;
            this.displayName = displayName;
            this.description = description;
        }

        public Material(Guid guid, string name)
            : base(guid, name)
        {

        }

        public Material(JObject jObject)
            : base(jObject)
        {
            
        }

        public Material(Material material)
            : base(material)
        {
            group = material.group;
            displayName = material.displayName;
            description = material.description;

            thermalConductivity = material.thermalConductivity;
            specificHeatCapacity = material.specificHeatCapacity;
            density = material.density;
        }

        public Material(string name, Guid guid, Material material, string displayName, string description)
            : base(name, guid, material)
        {
            group = material.group;
            this.displayName = displayName;
            this.description = description;

            thermalConductivity = material.thermalConductivity;
            specificHeatCapacity = material.specificHeatCapacity;
            density = material.density;
        }

        public string Group
        {
            get
            {
                return group;
            }
        }

        public string DisplayName
        {
            get
            {
                return displayName;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        /// <summary>
        /// Material Thermal Conductivity [W/mK]
        /// </summary>
        public double ThermalConductivity
        {
            get
            {
                return thermalConductivity;
            }
        }

        /// <summary>
        /// Specific Heat Capcity of Material [J/kgK]
        /// </summary>
        public double SpecificHeatCapacity
        {
            get
            {
                return specificHeatCapacity;
            }
        }

        /// <summary>
        /// Material Density [kg/m3]
        /// </summary>
        public double Density
        {
            get
            {
                return density;
            }
        }

        public MaterialType MaterialType
        {
            get
            {
                if (this is GasMaterial)
                    return MaterialType.Gas;

                if (this is TransparentMaterial)
                    return MaterialType.Transparent;

                if (this is OpaqueMaterial)
                    return MaterialType.Opaque;

                return MaterialType.Undefined;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Group"))
                group = jObject.Value<string>("Group");

            if (jObject.ContainsKey("DisplayName"))
                displayName = jObject.Value<string>("DisplayName");

            if (jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

            if(jObject.ContainsKey("ThermalConductivity"))
                thermalConductivity = jObject.Value<double>("ThermalConductivity");

            if (jObject.ContainsKey("SpecificHeatCapacity"))
                specificHeatCapacity = jObject.Value<double>("SpecificHeatCapacity");

            if (jObject.ContainsKey("Density"))
                density = jObject.Value<double>("Density");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (group != null)
                jObject.Add("Group", group);

            if (displayName != null)
                jObject.Add("DisplayName", displayName);

            if (description != null)
                jObject.Add("Description", description);

            if (!double.IsNaN(thermalConductivity))
                jObject.Add("ThermalConductivity", thermalConductivity);

            if (!double.IsNaN(specificHeatCapacity))
                jObject.Add("SpecificHeatCapacity", specificHeatCapacity);

            if (!double.IsNaN(density))
                jObject.Add("Density", density);

            return jObject;
        }
    }
}