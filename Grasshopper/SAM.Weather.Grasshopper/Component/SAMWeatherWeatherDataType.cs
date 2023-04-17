using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Weather.Grasshopper
{
    public class SAMAnalyticalWeatherDataType : GH_SAMEnumComponent<WeatherDataType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d41c0352-d17c-43d8-962d-ef05ad8d9343");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Zone Type Enum Component
        /// </summary>
        public SAMAnalyticalWeatherDataType()
          : base("SAMWeather.WeatherDataType", "SAMWeather.WeatherDataType",
              "Select Weather Data Type",
              "SAM", "Weather")
        {
        }
    }
}