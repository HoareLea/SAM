using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Geometry.Object
{
    public class TextAppearance : Appearance
    {
        public double Height { get; set; }
        
        public string FontFamilyName { get; set; }

        public TextAppearance(Color color, double height, string fontFamilyName) 
            : base(color)
        {
            Height = height;
            FontFamilyName = fontFamilyName;
        }

        public TextAppearance(JObject jObject)
            :base(jObject)
        {

        }

        public TextAppearance(TextAppearance textAppearance)
            :base(textAppearance)
        {
            if(textAppearance != null)
            {
                Height = textAppearance.Height;
                FontFamilyName = textAppearance.FontFamilyName;
            }
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if (!double.IsNaN(Height))
            {
                jObject.Add("Height", Height);
            }

            if(!string.IsNullOrEmpty(FontFamilyName))
            {
                jObject.Add("FontFamilyName", FontFamilyName);
            }

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Height"))
            {
                Height = jObject.Value<double>("Height");
            }

            if (jObject.ContainsKey("FontFamilyName"))
            {
                FontFamilyName = jObject.Value<string>("FontFamilyName");
            }

            return true;
        }
    }
}
