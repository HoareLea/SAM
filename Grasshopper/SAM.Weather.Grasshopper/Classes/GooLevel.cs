using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Weather.Grasshopper
{
    public class GooWeatherData : GooSAMObject<WeatherData>
    {
        public GooWeatherData()
            : base()
        {
        }

        public GooWeatherData(WeatherData weatherData)
            : base(weatherData)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooWeatherData(Value);
        }
    }

    public class GooWeatherDataParam : GH_PersistentParam<GooWeatherData>, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("3de17bc3-6f46-4b97-ad5a-ce311ac9c02f");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooWeatherDataParam()
            : base(typeof(WeatherData).Name, typeof(WeatherData).Name, typeof(WeatherData).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooWeatherData> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooWeatherData value)
        {
            throw new NotImplementedException();
        }
    }
}