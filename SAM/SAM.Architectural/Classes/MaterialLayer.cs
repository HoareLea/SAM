﻿using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Architectural
{
    public class MaterialLayer : IJSAMObject, IArchitecturalObject
    {
        private string name;
        private double thickness;

        public MaterialLayer(string name, double thickness)
        {
            this.thickness = thickness;
            this.name = name;
        }

        public MaterialLayer(MaterialLayer materialLayer)
        {
            thickness = materialLayer.thickness;
            name = materialLayer.name;
        }

        public MaterialLayer(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
        }
        public static bool operator !=(MaterialLayer materialLayer_1, MaterialLayer materialLayer_2)
        {
            return materialLayer_1?.name != materialLayer_2?.name || materialLayer_1?.thickness != materialLayer_2?.thickness;
        }

        public static bool operator ==(MaterialLayer materialLayer_1, MaterialLayer materialLayer_2)
        {
            return materialLayer_1?.name == materialLayer_2?.name && materialLayer_1?.thickness == materialLayer_2?.thickness;
        }

        public override bool Equals(object obj)
        {
            MaterialLayer materialLayer = obj as MaterialLayer;
            if (materialLayer == null)
            {
                return false;
            }

            return materialLayer.name == name && materialLayer.thickness == thickness;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Thickness"))
                thickness = jObject.Value<double>("Thickness");

            if (jObject.ContainsKey("Name"))
                name = jObject.Value<string>("Name");

            return true;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(name, thickness).GetHashCode();
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
                jObject.Add("Name", name);

            if (!double.IsNaN(thickness))
                jObject.Add("Thickness", thickness);

            return jObject;
        }
    }
}