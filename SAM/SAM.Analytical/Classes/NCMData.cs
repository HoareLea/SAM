using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Data for National Calculation Method
    /// </summary>
    public class NCMData : ParameterizedSAMObject
    {
        public string Type { get; set; } = "Unoccupied and Unconditioned";

        public NCMSystemType SystemType { get; set; } = NCMSystemType.NaturalVentilation;

        public LightingOccupancyControls LightingOccupancyControls { get; set; } = LightingOccupancyControls.None;

        public LightingPhotoelectricControls LightingPhotoelectricControls { get; set; } = LightingPhotoelectricControls.None;

        public NCMCountry Country { get; set; } = NCMCountry.England;

        public bool LightingPhotoelectricBackSpaceSensor { get; set; } = false;

        public bool LightingPhotoelectricControlsTimeSwitch { get; set; } = false;

        public bool LightingDaylightFactorMethod { get; set; } = false;

        public bool IsMainsGasAvailable { get; set; } = false;

        /// <summary>
        /// Lighting Photoelectric Parasitic Power [W]
        /// </summary>
        public double LightingPhotoelectricParasiticPower { get; set; } = 0.1;

        public double AirPermeability { get; set; } = 0;

        public string Description { get; set; } = null;

        public NCMData()
            :base()
        {

        }

        public NCMData(JObject jObject)
            : base(jObject)
        {
        }

        public NCMData(NCMData nCMData)
            :base(nCMData)
        {
            if(nCMData != null)
            {
                LightingOccupancyControls = nCMData.LightingOccupancyControls;
                LightingPhotoelectricControls = nCMData.LightingPhotoelectricControls;
                Country = nCMData.Country;
                LightingPhotoelectricBackSpaceSensor = nCMData.LightingPhotoelectricBackSpaceSensor;
                LightingPhotoelectricControlsTimeSwitch = nCMData.LightingPhotoelectricControlsTimeSwitch;
                LightingDaylightFactorMethod = nCMData.LightingDaylightFactorMethod;
                IsMainsGasAvailable = nCMData.IsMainsGasAvailable;
                LightingPhotoelectricParasiticPower = nCMData.LightingPhotoelectricParasiticPower;
                AirPermeability = nCMData.AirPermeability;
                Type = nCMData.Type;
                SystemType = nCMData.SystemType;
                Description = nCMData.Description;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            
            if(!result)
            {
                return false;
            }

            if (jObject.ContainsKey("Type"))
            {
                Type = jObject.Value<string>("Type");
            }

            if (jObject.ContainsKey("SystemType"))
            {
                string systemType_String = jObject.Value<string>("SystemType");
                SystemType = Core.Query.Enum<NCMSystemType>(systemType_String);
            }

            if (jObject.ContainsKey("LightingOccupancyControls"))
            {
                string lightingOccupancyControls_String = jObject.Value<string>("LightingOccupancyControls");
                LightingOccupancyControls = Core.Query.Enum<LightingOccupancyControls>(lightingOccupancyControls_String);
            }

            if (jObject.ContainsKey("LightingPhotoelectricControls"))
            {
                string lightingPhotoelectricControls_String = jObject.Value<string>("LightingPhotoelectricControls");
                LightingPhotoelectricControls = Core.Query.Enum<LightingPhotoelectricControls>(lightingPhotoelectricControls_String);
            }

            if (jObject.ContainsKey("Country"))
            {
                string Country_String = jObject.Value<string>("Country");
                Country = Core.Query.Enum<NCMCountry>(Country_String);
            }

            if (jObject.ContainsKey("LightingPhotoelectricBackSpaceSensor"))
            {
                LightingPhotoelectricBackSpaceSensor = jObject.Value<bool>("LightingPhotoelectricBackSpaceSensor");
            }

            if (jObject.ContainsKey("LightingPhotoelectricControlsTimeSwitch"))
            {
                LightingPhotoelectricControlsTimeSwitch = jObject.Value<bool>("LightingPhotoelectricControlsTimeSwitch");
            }

            if (jObject.ContainsKey("LightingDaylightFactorMethod"))
            {
                LightingDaylightFactorMethod = jObject.Value<bool>("LightingDaylightFactorMethod");
            }

            if (jObject.ContainsKey("IsMainsGasAvailable"))
            {
                IsMainsGasAvailable = jObject.Value<bool>("IsMainsGasAvailable");
            }

            if (jObject.ContainsKey("LightingPhotoelectricParasiticPower"))
            {
                LightingPhotoelectricParasiticPower = jObject.Value<double>("LightingPhotoelectricParasiticPower");
            }

            if (jObject.ContainsKey("AirPermeability"))
            {
                AirPermeability = jObject.Value<double>("AirPermeability");
            }

            if (jObject.ContainsKey("Description"))
            {
                Description = jObject.Value<string>("Description");
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if (Type != null)
            {
                jObject.Add("Type", Type);
            }

            if (SystemType != NCMSystemType.Undefined)
            {
                jObject.Add("SystemType", SystemType.ToString());
            }

            if (LightingOccupancyControls != LightingOccupancyControls.Undefined)
            {
                jObject.Add("LightingOccupancyControls", LightingOccupancyControls.ToString());
            }

            if (LightingPhotoelectricControls != LightingPhotoelectricControls.Undefined)
            {
                jObject.Add("LightingPhotoelectricControls", LightingPhotoelectricControls.ToString());
            }

            if (Country != NCMCountry.Undefined)
            {
                jObject.Add("Country", Country.ToString());
            }

            jObject.Add("LightingPhotoelectricBackSpaceSensor", LightingPhotoelectricBackSpaceSensor);
            jObject.Add("LightingPhotoelectricControlsTimeSwitch", LightingPhotoelectricControlsTimeSwitch);
            jObject.Add("LightingDaylightFactorMethod", LightingDaylightFactorMethod);
            jObject.Add("IsMainsGasAvailable", IsMainsGasAvailable);

            if (!double.IsNaN(LightingPhotoelectricParasiticPower))
            {
                jObject.Add("LightingPhotoelectricParasiticPower", LightingPhotoelectricParasiticPower);
            }

            if (!double.IsNaN(AirPermeability))
            {
                jObject.Add("AirPermeability", AirPermeability);
            }

            if (Description != null)
            {
                jObject.Add("Description", Description);
            }

            return jObject;
        }
    }
}