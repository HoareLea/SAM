using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace SAM.Weather.Grasshopper
{
    public class GooWeatherYear : GooJSAMObject<WeatherYear>
    {
        public GooWeatherYear()
            : base()
        {
        }

        public GooWeatherYear(WeatherYear weatherYear)
            : base(weatherYear)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooWeatherYear(Value);
        }
    }

    public class GooWeatherYearParam : GH_PersistentParam<GooWeatherYear>, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("3778cda9-ca09-4d52-b9fb-ed23780dc0e4");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooWeatherYearParam()
            : base(typeof(WeatherYear).Name, typeof(WeatherYear).Name, typeof(WeatherYear).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooWeatherYear> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooWeatherYear value)
        {
            throw new NotImplementedException();
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}