// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class SystemTemplate : IJSAMObject, IAnalyticalObject
    {
        private string controls;
        private string cooling;
        private string heating;
        private string plantRoom;
        private string ventilation;
        private string version;

        public SystemTemplate()
        {
            
        }

        public SystemTemplate(string ventilation, string heating, string cooling, string plantRoom, string controls, string version)
        {
            Ventilation = ventilation;
            Heating = heating;
            Cooling = cooling;
            PlantRoom = plantRoom;
            Controls = controls;
            Version = version;

        }

        public SystemTemplate(SystemTemplate systemTemplate)
        {
            if (systemTemplate != null)
            {
                ventilation = systemTemplate.ventilation;
                heating = systemTemplate.heating;
                cooling = systemTemplate.cooling;
                plantRoom = systemTemplate.plantRoom;
                controls = systemTemplate.controls;
                version = systemTemplate.version;
            }
        }

        public SystemTemplate(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Controls
        {
            get
            {
                return controls;
            }

            set
            {
                controls = value?.Replace(" ", string.Empty);
            }
        }

        public string Cooling
        {
            get
            {
                return cooling;
            }

            set
            {
                cooling = value?.Replace(" ", string.Empty);
            }
        }

        public string Heating
        {
            get
            {
                return heating;
            }

            set
            {
                heating = value?.Replace(" ", string.Empty);
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ToString());
            }
        }
        
        public string PlantRoom
        {
            get
            {
                return plantRoom;
            }

            set
            {
                plantRoom = value?.Replace(" ", string.Empty);
            }
        }

        public string Ventilation
        {
            get
            {
                return ventilation;
            }

            set
            {
                ventilation = value?.Replace(" ", string.Empty);
            }
        }
        
        public string Version
        {
            get
            {
                return version;
            }

            set
            {
                version = value?.Replace(" ", string.Empty);
            }
        }

        public bool IsUnheated()
        {
            return string.IsNullOrWhiteSpace(heating) || heating == "UH";
        }

        public bool IsUncooled()
        {
            return string.IsNullOrWhiteSpace(cooling) || heating == "UC";
        }

        public bool IsUnventilated()
        {
            return string.IsNullOrWhiteSpace(ventilation) || ventilation == "UV";
        }

        public override bool Equals(object obj)
        {

            return base.Equals(obj);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Ventilation"))
            {
                ventilation = jObject.Value<string>("Ventilation");
            }

            if (jObject.ContainsKey("Heating"))
            {
                heating = jObject.Value<string>("Heating");
            }

            if (jObject.ContainsKey("Cooling"))
            {
                cooling = jObject.Value<string>("Cooling");
            }

            if (jObject.ContainsKey("PlantRoom"))
            {
                plantRoom = jObject.Value<string>("PlantRoom");
            }

            if (jObject.ContainsKey("Controls"))
            {
                controls = jObject.Value<string>("Controls");
            }

            if (jObject.ContainsKey("Version"))
            {
                version = jObject.Value<string>("Version");
            }


            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (ventilation != null)
            {
                jObject.Add("Ventilation", ventilation);
            }

            if (heating != null)
            {
                jObject.Add("Heating", heating);
            }

            if (cooling != null)
            {
                jObject.Add("Cooling", cooling);
            }

            if (plantRoom != null)
            {
                jObject.Add("PlantRoom", plantRoom);
            }

            if (controls != null)
            {
                jObject.Add("Controls", controls);
            }

            if (version != null)
            {
                jObject.Add("Version", version);
            }


            return jObject;
        }

        public override string ToString()
        {
            List<string> strings = [];
            
            if(!string.IsNullOrWhiteSpace(ventilation))
            {
                strings.Add("V." + ventilation);
            }

            if (!string.IsNullOrWhiteSpace(heating))
            {
                strings.Add("H." + heating);
            }

            if (!string.IsNullOrWhiteSpace(cooling))
            {
                strings.Add("C." + cooling);
            }

            if (!string.IsNullOrWhiteSpace(plantRoom))
            {
                strings.Add("PR." + plantRoom);
            }

            if (!string.IsNullOrWhiteSpace(controls))
            {
                strings.Add("CTL." + controls);
            }

            if (!string.IsNullOrWhiteSpace(version))
            {
                strings.Add("VER." + version);
            }

            return string.Join("_", strings);
        }
    }
}
