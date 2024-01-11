using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Geometry
{
    public class SurfaceAppearance : Appearance
    {
        public CurveAppearance CurveAppearance { get; set; }
        
        public SurfaceAppearance(Color surfaceColor, Color curveColor, double curveThickness) 
            : base(surfaceColor)
        {
            CurveAppearance = new CurveAppearance(curveColor, curveThickness);
        }

        public SurfaceAppearance(JObject jObject)
            : base(jObject)
        {

        }

        public SurfaceAppearance(SurfaceAppearance surfaceAppearance)
            : base(surfaceAppearance)
        {
            CurveAppearance curveAppearance = surfaceAppearance?.CurveAppearance;
            if (curveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(curveAppearance);
            }
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if (CurveAppearance != null)
            {
                jObject.Add("CurveAppearance", CurveAppearance.ToJObject());
            }

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("CurveAppearance"))
            {
                CurveAppearance = new CurveAppearance(jObject.Value<JObject>("CurveAppearance"));
            }

            return true;
        }
    }
}
