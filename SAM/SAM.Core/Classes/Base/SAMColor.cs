using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Core
{
    public class SAMColor : IJSAMObject
    {
        private byte alpha;
        private byte red;
        private byte green;
        private byte blue;

        public SAMColor(System.Drawing.Color color)
        {
            alpha = color.A;
            red = color.R;
            green = color.G;
            blue = color.B;
        }

        public SAMColor(SAMColor sAMColor)
        {
            alpha = sAMColor.alpha;
            red = sAMColor.red;
            green = sAMColor.Green;
            blue = sAMColor.blue;
        }

        public SAMColor(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMColor(byte alpha, byte red, byte green, byte blue)
        {
            this.alpha = alpha;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public string Name
        {
            get
            {
                return ToColor().Name;
            }
        }

        public byte Alpha
        {
            get
            {
                return alpha;
            }
        }

        public byte Red
        {
            get
            {
                return red;
            }
        }

        public byte Green
        {
            get
            {
                return green;
            }
        }

        public byte Blue
        {
            get
            {
                return blue;
            }
        }

        public SAMColor Clone()
        {
            return new SAMColor(this);
        }

        public Color ToColor()
        {
            return System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            Color color = Color.Empty;
            if (jObject.ContainsKey("Name"))
                color = Convert.ToColor(jObject.Value<string>("Name"));

            if(color.Equals(Color.Empty))
            {
                alpha = jObject.Value<byte>("Alpha");
                red = jObject.Value<byte>("Red");
                green = jObject.Value<byte>("Green");
                blue = jObject.Value<byte>("Blue");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            string name = Name;
            if(string.IsNullOrWhiteSpace(name))
            {
                jObject.Add("Alpha", alpha);
                jObject.Add("Red", red);
                jObject.Add("Green", green);
                jObject.Add("Blue", blue);
            }
            else
            {
                jObject.Add("Name", name);
            }

            return jObject;
        }

        public override string ToString()
        {
            return ToColor().Name;
        }
    }
}