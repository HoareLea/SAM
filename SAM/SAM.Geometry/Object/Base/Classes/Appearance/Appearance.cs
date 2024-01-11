using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Geometry.Object
{
    public abstract class Appearance :IAppearance
    {
        public Color Color { get; set; }

        public double Opacity { get; set; } = 1;

        public bool Visible { get; set; } = true;

        public Appearance(Color color)
        {
            Color = color;
        }

        public Appearance(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Appearance(Appearance appearance)
        {
            if(appearance != null)
            {
                Color = appearance.Color;
                Opacity = appearance.Opacity;
                Visible = appearance.Visible;
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Color"))
            {
                Core.SAMColor sAMColor = new Core.SAMColor(jObject.Value<JObject>("Color"));
                if (sAMColor != null)
                {
                    Color = Color.FromArgb(sAMColor.Alpha, sAMColor.Red, sAMColor.Green, sAMColor.Blue);
                }
            }

            if (jObject.ContainsKey("Opacity"))
            {
                Opacity = jObject.Value<double>("Opacity");
            }

            if (jObject.ContainsKey("Visible"))
            {
                Visible = jObject.Value<bool>("Visible");
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            jObject.Add("Color", new Core.SAMColor(Color.A, Color.R, Color.G, Color.B).ToJObject());
            
            if(!double.IsNaN(Opacity))
            {
                jObject.Add("Opacity", Opacity);
            }

            jObject.Add("Visible", Visible);

            return jObject;
        }
    }
}
