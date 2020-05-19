using Newtonsoft.Json.Linq;

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

        public SAMColor(byte alpha, byte red, byte green, byte blue)
        {
            this.alpha = alpha;
            this.red = red;
            this.green = green;
            this.blue = blue;
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

        public System.Drawing.Color ToColor()
        {
            return System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            alpha = jObject.Value<byte>("Alpha");
            red = jObject.Value<byte>("Red");
            green = jObject.Value<byte>("Green");
            blue = jObject.Value<byte>("Blue");
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("Alpha", alpha);
            jObject.Add("Red", red);
            jObject.Add("Green", green);
            jObject.Add("Blue", blue);

            return jObject;
        }
    }
}