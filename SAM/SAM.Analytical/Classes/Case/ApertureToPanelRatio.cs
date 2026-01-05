// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical.Classes
{
    public class ApertureToPanelRatio : IJSAMObject
    {
        private ApertureConstruction apertureConstruction;
        private Range<double> azimuthRange;
        private double ratio;

        public ApertureToPanelRatio(Range<double> azimuthRange, double ratio, ApertureConstruction apertureConstruction)
        {
            this.azimuthRange = azimuthRange;
            this.ratio = ratio;
            this.apertureConstruction = apertureConstruction;
        }

        public ApertureToPanelRatio(JObject jObject)
        {
            FromJObject(jObject);
        }

        public ApertureToPanelRatio(ApertureToPanelRatio apertureToPanelRatio)
        {
            if (apertureToPanelRatio != null)
            {
                azimuthRange = apertureToPanelRatio.AzimuthRange;
                ratio = apertureToPanelRatio.Ratio;
                apertureConstruction = apertureToPanelRatio.ApertureConstruction;
            }
        }

        public ApertureConstruction ApertureConstruction
        {
            get
            {
                return apertureConstruction;
            }

            set
            {
                apertureConstruction = value;
            }
        }

        public string ApertureConstructionName
        {
            get
            {
                return apertureConstruction?.Name;
            }
        }

        public Range<double> AzimuthRange
        {
            get
            {
                return azimuthRange;
            }
        }

        public double Max
        {
            get
            {
                if (azimuthRange is null)
                {
                    azimuthRange = new Range<double>(0.0, 0.0);
                }

                return azimuthRange.Max;
            }

            set
            {
                if (azimuthRange is null)
                {
                    azimuthRange = new Range<double>(value, value);
                }
                else
                {
                    azimuthRange = new Range<double>(azimuthRange.Min, value);
                }
            }
        }

        public double Min
        {
            get
            {
                if (azimuthRange is null)
                {
                    azimuthRange = new Range<double>(0.0, 0.0);
                }

                return azimuthRange.Min;
            }

            set
            {
                if (azimuthRange is null)
                {
                    azimuthRange = new Range<double>(value, value);
                }
                else
                {
                    azimuthRange = new Range<double>(value, azimuthRange.Max);
                }
            }
        }

        public double Ratio
        {
            get
            {
                return ratio;
            }

            set
            {
                ratio = value;
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Ratio"))
            {
                ratio = jObject.Value<double>("Ratio");
            }

            if (jObject.ContainsKey("AzimuthRange"))
            {
                azimuthRange = Core.Query.IJSAMObject<Range<double>>(jObject.Value<JObject>("AzimuthRange"));
            }

            if (jObject.ContainsKey("ApertureConstruction"))
            {
                apertureConstruction = Core.Query.IJSAMObject<ApertureConstruction>(jObject.Value<JObject>("ApertureConstruction"));
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject result = new();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (azimuthRange != null)
            {
                result.Add("AzimuthRange", azimuthRange.ToJObject());
            }

            if (double.IsNaN(ratio))
            {
                result.Add("Ratio", ratio);
            }

            if (apertureConstruction != null)
            {
                result.Add("ApertureConstruction", apertureConstruction.ToJObject());
            }

            return result;
        }
    }
}
