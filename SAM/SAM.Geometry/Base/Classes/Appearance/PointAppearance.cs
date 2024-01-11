using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Geometry
{
    public class PointAppearance : Appearance
    {
        public double Thickness { get; set; }

        public PointAppearance(Color color, double thickness) 
            : base(color)
        {
            Thickness = thickness;
        }

        public PointAppearance(JObject jObject)
            :base(jObject)
        {

        }

        public PointAppearance(PointAppearance pointAppearance)
            :base(pointAppearance)
        {
            if(pointAppearance != null)
            {
                Thickness = pointAppearance.Thickness;
            }
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if (!double.IsNaN(Thickness))
            {
                jObject.Add("Thickness", Thickness);
            }

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Thickness"))
            {
                Thickness = jObject.Value<double>("Thickness");
            }

            return true;
        }
    }
}
