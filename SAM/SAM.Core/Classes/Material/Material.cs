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
        private double specificHeat = double.NaN;
        private double density = double.NaN;
        private double vapourDiffusionFactor = double.NaN;
        
        public Material(string name)
            : base(name)
        {

        }

        public Material(string name, string group, string displayName, string description, double thermalConductivity, double specificHeat, double density)
            : base(name)
        {
            this.group = group;
            this.displayName = displayName;
            this.description = description;

            this.thermalConductivity = thermalConductivity;
            this.specificHeat = specificHeat;
            this.density = density;
        }

        public Material(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeat)
            :base(guid, name)
        {
            this.displayName = displayName;
            this.description = description;
            this.thermalConductivity = thermalConductivity;
            this.density = density;
            this.specificHeat = specificHeat;
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
        /// Specific Heat of Material [J/kgK]
        /// </summary>
        public double SpecificHeat
        {
            get
            {
                return specificHeat;
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

        /// <summary>
        /// The specific moisture diffusion resistance of the material [-]
        /// </summary>
        public double VapourDiffusionFactor
        {
            get
            {
                return vapourDiffusionFactor;
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

            if (jObject.ContainsKey("VapourDiffusionFactor"))
                vapourDiffusionFactor = jObject.Value<double>("VapourDiffusionFactor");

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

            if (!double.IsNaN(vapourDiffusionFactor))
                jObject.Add("VapourDiffusionFactor", vapourDiffusionFactor);

            if (!double.IsNaN(thermalConductivity))
                jObject.Add("Conductivity", thermalConductivity);

            if (!double.IsNaN(specificHeat))
                jObject.Add("SpecificHeat", specificHeat);

            if (!double.IsNaN(density))
                jObject.Add("Density", density);

            return jObject;
        }
    }
}